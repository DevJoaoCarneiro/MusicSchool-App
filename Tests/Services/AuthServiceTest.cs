using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Services
{
    public class AuthServiceTest
    {
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly ISecurityService _securityService = Substitute.For<ISecurityService>();
        private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

        private readonly AuthServices _service;

        public AuthServiceTest()
        {
            _service = new AuthServices(_userRepository, _securityService, _tokenService);
        }

        [Fact]
        public async Task Should_Return_Sucess_When_Request_Is_Valid()
        {
            // Arrange
            var request = new AuthRequestDTO
            {
                Email = "joao@email.com",
                Password = "123456789"

            };

            var enumUser = new UserRole();

            var response = new User(
                "João",
                "joao@email.com",
                "$2b$10$N9qo8uLOickgx2ZMRZo5i.Ul5pG8z0lYhQ0vFqzj8zVYQFzvY5x1e",
                UserRole.Admin
             );

            _userRepository.GetByEmailAsync(request.Email).Returns(response);

            _securityService.VerifyPassword(request.Password, response.PasswordHash).Returns(true);

            _tokenService.GenerateToken(response).Returns("mocked_token");

            // Act
            var result = await _service.LoginAsync(request);

            //Assert
            Assert.NotNull(result);
            Assert.Equal("Success", result.Status);
            Assert.Equal("Login successful", result.Message);            
        }

      
    }


}
