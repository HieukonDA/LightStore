// ===== ONION ARCHITECTURE NAMESPACES =====

// Domain Layer - Entities
global using TheLightStore.Domain.Entities.Auth;
global using TheLightStore.Domain.Entities.Blogs;
global using TheLightStore.Domain.Entities.Carts;
global using TheLightStore.Domain.Entities.Coupons;
global using TheLightStore.Domain.Entities.Notifications;
global using TheLightStore.Domain.Entities.Orders;
global using TheLightStore.Domain.Entities.Products;
global using TheLightStore.Domain.Entities.Reviews;
global using TheLightStore.Domain.Entities.Shared;
global using TheLightStore.Domain.Entities.Shipping;

// Application Layer - DTOs
global using TheLightStore.Application.DTOs.Address;
global using TheLightStore.Application.DTOs.Auth;
global using TheLightStore.Application.DTOs.Banners;
global using TheLightStore.Application.DTOs.Blogs;
global using TheLightStore.Application.DTOs.Carts;
global using TheLightStore.Application.DTOs.Categories;
global using TheLightStore.Application.DTOs.Dashboard;
global using TheLightStore.Application.DTOs.GHN;
global using TheLightStore.Application.DTOs.Inventory;
global using TheLightStore.Application.DTOs.Momo;
global using TheLightStore.Application.DTOs.Notifications;
global using TheLightStore.Application.DTOs.Orders;
global using TheLightStore.Application.DTOs.Paging;
global using TheLightStore.Application.DTOs.Products;
global using TheLightStore.Application.DTOs.Search;

// Application Layer - Interfaces
global using TheLightStore.Application.Interfaces;

// Infrastructure Layer
global using TheLightStore.Infrastructure.Persistence;

// ===== SYSTEM NAMESPACES =====
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Threading.RateLimiting;
global using System.Text.Json.Serialization;
global using Microsoft.AspNetCore.SignalR;