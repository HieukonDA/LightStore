// namespaces
global using TheLightStore.Datas;

global using TheLightStore.Models.Auth;
global using TheLightStore.Dtos.Auth;
global using TheLightStore.Interfaces.Auth;


global using TheLightStore.Models.Categories;
global using TheLightStore.Dtos.Category;
global using TheLightStore.Interfaces.Category;
global using TheLightStore.Repositories.Category;


global using TheLightStore.Models.Attributes;


global using TheLightStore.Models.Products;
global using TheLightStore.Dtos.Products;
global using TheLightStore.Interfaces.Products;
global using TheLightStore.Repositories.Products;
global using TheLightStore.Services.Products;


global using TheLightStore.Models.Inventories;


global using TheLightStore.Models.Orders_Carts;


global using TheLightStore.Models.Blogs;


global using TheLightStore.Models.Coupons_Discounts;


global using TheLightStore.Models.ProductReviews;


global using TheLightStore.Interfaces.Repository;
global using TheLightStore.Services;
global using TheLightStore.Dtos.Paging;

global using TheLightStore.Models.Shipping;
global using TheLightStore.Models.System;




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