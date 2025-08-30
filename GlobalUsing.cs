// namespaces
global using TheLightStore.Datas;
global using TheLightStore.Models.Auth;
global using TheLightStore.Dtos.Auth;
global using TheLightStore.Interfaces.Auth;
global using TheLightStore.Interfaces.Repository;
global using TheLightStore.Services;
global using TheLightStore.Models.Product;
global using TheLightStore.Models.Category;
global using TheLightStore.Models.Attribute;
global using TheLightStore.Dtos.Category;
global using TheLightStore.Interfaces.Category;
global using TheLightStore.Dtos.Paging;
global using TheLightStore.Repositories.Category;


// system
global using Microsoft.EntityFrameworkCore;
global using System.Text;
global using System.Threading.RateLimiting;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.IdentityModel.Tokens;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using TheLightStore.Repositories.Auth;
global using System.ComponentModel.DataAnnotations.Schema;
global using Microsoft.AspNetCore.Mvc;