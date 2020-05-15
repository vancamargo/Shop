using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("users")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices] DataContext context)
        {
            var users = await context.Users.AsNoTracking().ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
       // [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Post([FromBody] User model,
           [FromServices] DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                //força o usuário a ser sempre "funcionário"
                model.Role = "employee";
                await context.SaveChangesAsync();
                model.Password = "";
                return Ok(model);
                
                //Esconde a senha
                

            }
            catch
            {
                return BadRequest(ModelState);
            }

        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult<List<User>>> Put(int id, [FromBody] User model,
            [FromServices] DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if(id != model.Id)
                return NotFound(new { message = "Usuário não encontrado" });
            try
            {
                context.Entry(model).State = EntityState.Modified;

                await context.SaveChangesAsync();
                return Ok(model);
            }
          

            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível criar o usuário" });
            }
        }



        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody] User model,
            [FromServices] DataContext context)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(x => x.Username == model.Username && x.Password == model.Password)
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválida" });

            var token = TokenService.GenerateToken(user);
            user.Password = "";
            return new
            {
                user = user,
                token = token
            };
        }



    

    }

}