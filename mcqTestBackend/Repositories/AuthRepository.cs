using mcqTestBackend.Dtos.Auth;
using mcqTestBackend.Model;
using mcqTestBackend.Repositories.IRepository;
using mcqTestBackend.Service.IService;
using mcqTestBackend.Utility;
using System.Linq;
using Microsoft.AspNetCore.Identity;


namespace mcqTestBackend.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthRepository(
                        UserManager<ApplicationUser> userManager,
                        RoleManager<IdentityRole> roleManager,
                        ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<RegisterUserResponseDTO> RegisterAsync(RegisterUserDTO model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                throw new Exception("User already exists with this email.");
            }

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception("Failed to assign user to role: " + errors);
            }

            // Ensure roles exist
            if (!await _roleManager.RoleExistsAsync(SD.Role_User))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_User));
            }

            if (!await _roleManager.RoleExistsAsync(SD.Role_Admin))
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, SD.Role_User);

            // Add user to role
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new Exception("Failed to assign user to role: " + errors);
            }

            // Generate JWT token
            var token = await _tokenService.CreateToken(user, SD.Role_User);

            return new RegisterUserResponseDTO
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Token = token
            };
        }


        public async Task<LoginUserResponseDTO> LoginAsync(LoginUserDTO model)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password.");
            }

            // Check password
            var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordValid)
            {
                throw new Exception("Invalid email or password.");
            }

            // Get roles of user
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? SD.Role_User; 

            // Generate JWT token
            var token = await _tokenService.CreateToken(user, userRole);

            return new LoginUserResponseDTO
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Token = token,
                Role = userRole
            };
        }


    }
}
