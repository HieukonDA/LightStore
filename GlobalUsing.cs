// Clean Architecture - Only essential global usings
// Old namespaces removed to avoid conflicts with Application layer

// Database
global using TheLightStore.Datas;

// System
global using Microsoft.EntityFrameworkCore;
global using System.Text;
global using System.Threading.RateLimiting;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.IdentityModel.Tokens;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.ComponentModel.DataAnnotations.Schema;
global using Microsoft.AspNetCore.Mvc;
global using System.Text.Json;
global using Microsoft.AspNetCore.Authorization;
global using System.ComponentModel.DataAnnotations;
global using System.Text.Json.Serialization;
global using Microsoft.AspNetCore.SignalR;
