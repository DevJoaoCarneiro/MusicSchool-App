import http from 'k6/http';
import { check, group } from 'k6';
import { htmlReport } from 'https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';

const baseUrl = (__ENV.BASE_URL || 'https://localhost:7020').replace(/\/$/, '');
const email = __ENV.ADMIN_EMAIL || 'admin@musicschool.local';
const password = __ENV.ADMIN_PASSWORD;

export const options = {
  scenarios: {
    api_read_load: {
      executor: 'constant-vus',
      vus: Number(__ENV.VUS || 10),
      duration: __ENV.DURATION || '30s',
    },
  },
  thresholds: {
    'http_req_duration{endpoint:students}': ['p(95)<500'],
    'http_req_duration{endpoint:teachers}': ['p(95)<500'],
    'http_req_duration{endpoint:lessons}': ['p(95)<500'],
    http_req_failed: ['rate<0.01'],
  },
  insecureSkipTLSVerify: __ENV.INSECURE_TLS === 'true',
};

export function setup() {
  if (!password) {
    throw new Error('ADMIN_PASSWORD must be provided.');
  }

  const response = http.post(
    `${baseUrl}/api/auth/login`,
    JSON.stringify({ email, password }),
    {
      headers: { 'Content-Type': 'application/json' },
      tags: { endpoint: 'login' },
    },
  );

  let token = null;
  if (response.body) {
    try {
      token = response.json('data.token');
    } catch (error) {
      token = null;
    }
  }

  check(response, {
    'login returns 200': (res) => res.status === 200,
    'login returns a token': () => Boolean(token),
  });

  if (response.status !== 200 || !token) {
    throw new Error(
      `Login failed with status ${response.status}. Is the API running at ${baseUrl}?`,
    );
  }

  return { token };
}

export default function (data) {
  const headers = {
    Authorization: `Bearer ${data.token}`,
  };

  group('students', () => {
    const response = http.get(
      `${baseUrl}/api/Student?page=1&pageSize=20`,
      {
        headers,
        tags: { endpoint: 'students' },
      },
    );

    check(response, {
      'students returns 200': (res) => res.status === 200,
    });
  });

  group('teachers', () => {
    const response = http.get(
      `${baseUrl}/api/teachers?page=1&pageSize=20`,
      {
        headers,
        tags: { endpoint: 'teachers' },
      },
    );

    check(response, {
      'teachers returns 200': (res) => res.status === 200,
    });
  });

  group('lessons', () => {
    const from = new Date(Date.now() + 24 * 60 * 60 * 1000);
    const to = new Date(from.getTime() + 6 * 24 * 60 * 60 * 1000);
    const fromDate = from.toISOString().slice(0, 10);
    const toDate = to.toISOString().slice(0, 10);

    const response = http.get(
      `${baseUrl}/api/lessons?from=${fromDate}&to=${toDate}`,
      {
        headers,
        tags: { endpoint: 'lessons' },
      },
    );

    check(response, {
      'lessons returns 200': (res) => res.status === 200,
    });
  });
}

export function handleSummary(data) {
  return {
    'artifacts/load-test-summary.json': JSON.stringify(data, null, 2),
    'artifacts/load-test-report.html': htmlReport(data),
    stdout: textSummary(data, { indent: ' ', enableColors: false }),
  };
}
