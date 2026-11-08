import { execFileSync } from 'node:child_process'
import fs from 'node:fs'
import path from 'node:path'
import type { FullResult, Reporter, TestCase, TestResult } from '@playwright/test/reporter'

interface Clip {
  file: string
  spec: string
  title: string
  status: string
  duration: number
}

function findFullFfmpeg() {
  const candidates = [process.env.FFMPEG_PATH, 'ffmpeg'].filter(Boolean) as string[]

  for (const candidate of candidates) {
    try {
      const demuxers = execFileSync(candidate, ['-hide_banner', '-demuxers'], { encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] })
      if (/\bconcat\b/.test(demuxers)) return candidate
    } catch {
      continue
    }
  }

  return null
}

function escapeHtml(value: string) {
  return value.replace(/[&<>"']/g, (char) => `&#${char.charCodeAt(0)};`)
}

function playerHtml(clips: Clip[], status: string) {
  const items = clips.map((clip, index) => ({
    src: `clips/${String(index + 1).padStart(2, '0')}.webm`,
    spec: clip.spec,
    title: clip.title,
    status: clip.status,
    seconds: Math.round(clip.duration / 1000),
  }))

  const passed = clips.filter((clip) => clip.status === 'passed').length

  return `<!doctype html>
<html lang="pt-BR">
<head>
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>Execução dos testes E2E</title>
<style>
  :root { color-scheme: light dark; --bg: #f8f5f0; --panel: #fdfbf8; --text: #1f1714; --muted: #6b6058; --border: #e7e0d5; --accent: #5e2c2a; --ok: #2f7a4a; --fail: #c0392b; }
  @media (prefers-color-scheme: dark) { :root { --bg: #17110f; --panel: #211916; --text: #f1e9e3; --muted: #a8988e; --border: #3a2c27; --accent: #e3a24f; } }
  * { box-sizing: border-box; }
  body { margin: 0; font-family: system-ui, sans-serif; background: var(--bg); color: var(--text); }
  header { padding: 16px 24px; border-bottom: 1px solid var(--border); display: flex; flex-wrap: wrap; gap: 8px 24px; align-items: baseline; }
  h1 { margin: 0; font-size: 20px; }
  .summary { color: var(--muted); font-size: 14px; }
  main { display: grid; grid-template-columns: minmax(0, 1fr) 380px; gap: 16px; padding: 16px 24px; }
  video { width: 100%; background: #000; border-radius: 8px; }
  .now { margin-top: 10px; font-size: 15px; }
  .now small { display: block; color: var(--muted); }
  .controls { display: flex; gap: 8px; margin-top: 10px; }
  button { font: inherit; padding: 6px 12px; border-radius: 6px; border: 1px solid var(--border); background: var(--panel); color: var(--text); cursor: pointer; }
  ol { list-style: none; margin: 0; padding: 0; max-height: calc(100vh - 110px); overflow: auto; border: 1px solid var(--border); border-radius: 8px; background: var(--panel); }
  li button { width: 100%; text-align: left; border: 0; border-bottom: 1px solid var(--border); border-radius: 0; display: grid; grid-template-columns: 28px 1fr auto; gap: 8px; padding: 8px 12px; background: transparent; }
  li button[aria-current="true"] { background: color-mix(in srgb, var(--accent) 14%, transparent); }
  .num { color: var(--muted); font-variant-numeric: tabular-nums; }
  .spec { display: block; font-size: 12px; color: var(--muted); }
  .badge { font-size: 12px; font-weight: 600; }
  .passed { color: var(--ok); } .failed, .timedOut, .interrupted { color: var(--fail); }
  @media (max-width: 900px) { main { grid-template-columns: 1fr; } ol { max-height: none; } }
</style>
</head>
<body>
<header>
  <h1>Execução dos testes E2E</h1>
  <span class="summary">${passed} de ${clips.length} passaram · resultado: ${escapeHtml(status)}</span>
</header>
<main>
  <section>
    <video id="player" controls autoplay muted></video>
    <div class="now" id="now"></div>
    <div class="controls">
      <button id="prev" type="button">Anterior</button>
      <button id="next" type="button">Próximo</button>
      <label><input id="auto" type="checkbox" checked /> tocar em sequência</label>
    </div>
  </section>
  <ol id="list"></ol>
</main>
<script>
  const clips = ${JSON.stringify(items)};
  const player = document.getElementById('player');
  const list = document.getElementById('list');
  const now = document.getElementById('now');
  const auto = document.getElementById('auto');
  let current = 0;

  const label = { passed: 'passou', failed: 'falhou', timedOut: 'tempo esgotado', interrupted: 'interrompido', skipped: 'pulado' };

  clips.forEach((clip, index) => {
    const li = document.createElement('li');
    const button = document.createElement('button');
    button.type = 'button';
    button.innerHTML = '<span class="num">' + (index + 1) + '</span><span><span class="spec"></span><span class="title"></span></span><span class="badge ' + clip.status + '">' + (label[clip.status] || clip.status) + '</span>';
    button.querySelector('.spec').textContent = clip.spec;
    button.querySelector('.title').textContent = clip.title + ' (' + clip.seconds + 's)';
    button.addEventListener('click', () => play(index));
    li.appendChild(button);
    list.appendChild(li);
  });

  function play(index) {
    current = index;
    const clip = clips[index];
    player.src = clip.src;
    player.play().catch(() => {});
    now.innerHTML = '';
    const small = document.createElement('small');
    small.textContent = (index + 1) + ' de ' + clips.length + ' · ' + clip.spec;
    now.append(small, document.createTextNode(clip.title));
    list.querySelectorAll('button').forEach((button, i) => button.setAttribute('aria-current', String(i === index)));
    list.children[index].scrollIntoView({ block: 'nearest' });
  }

  player.addEventListener('ended', () => { if (auto.checked && current < clips.length - 1) play(current + 1); });
  document.getElementById('prev').addEventListener('click', () => play(Math.max(0, current - 1)));
  document.getElementById('next').addEventListener('click', () => play(Math.min(clips.length - 1, current + 1)));
  play(0);
</script>
</body>
</html>
`
}

export default class VideoReporter implements Reporter {
  private readonly clips: Clip[] = []
  private readonly outputDir = path.join(process.cwd(), 'test-results', 'video')

  onTestEnd(test: TestCase, result: TestResult) {
    const video = result.attachments.find((attachment) => attachment.name === 'video' && attachment.path)
    if (!video?.path) return

    const [, , file, ...titles] = test.titlePath()
    this.clips.push({
      file: video.path,
      spec: file ?? '',
      title: titles.join(' › '),
      status: result.status,
      duration: result.duration,
    })
  }

  onEnd(result: FullResult) {
    const clips = this.clips.filter((clip) => fs.existsSync(clip.file))
    if (clips.length === 0) return

    const clipsDir = path.join(this.outputDir, 'clips')
    fs.rmSync(this.outputDir, { recursive: true, force: true })
    fs.mkdirSync(clipsDir, { recursive: true })

    clips.forEach((clip, index) => {
      fs.copyFileSync(clip.file, path.join(clipsDir, `${String(index + 1).padStart(2, '0')}.webm`))
    })

    const player = path.join(this.outputDir, 'index.html')
    fs.writeFileSync(player, playerHtml(clips, result.status))
    console.log(`\n[video] ${clips.length} testes gravados. Player com todos em sequência: ${player}`)

    const ffmpeg = findFullFfmpeg()
    if (!ffmpeg) {
      console.log('[video] Para gerar também um único arquivo .webm, instale o ffmpeg completo (ou defina FFMPEG_PATH).')
      return
    }

    const listFile = path.join(this.outputDir, 'lista.txt')
    const output = path.join(this.outputDir, `execucao-completa-${result.status}.webm`)
    fs.writeFileSync(listFile, clips.map((_, index) => `file 'clips/${String(index + 1).padStart(2, '0')}.webm'`).join('\n'))

    try {
      execFileSync(ffmpeg, ['-y', '-loglevel', 'error', '-f', 'concat', '-safe', '0', '-i', listFile, '-c', 'copy', output], {
        cwd: this.outputDir,
      })
      console.log(`[video] Vídeo único: ${output}`)
    } catch (error) {
      console.log(`[video] Não foi possível gerar o vídeo único: ${(error as Error).message}`)
    } finally {
      fs.rmSync(listFile, { force: true })
    }
  }

  printsToStdio() {
    return false
  }
}
