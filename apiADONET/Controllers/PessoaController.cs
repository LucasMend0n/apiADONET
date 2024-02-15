using apiADONET.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace apiADONET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoaController : ControllerBase
    {
        private readonly IConfiguration _config;
        public PessoaController(IConfiguration configuration)
        {
            _config = configuration;

        }

        [HttpGet]
        [Route("/pessoa")]
        public IActionResult GetPessoas()
        {
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(_config.GetConnectionString("Default"));
            SqlCommand cmd = new SqlCommand("p_sel_pessoa", con);
            SqlDataAdapter adp = new SqlDataAdapter(cmd);

            adp.Fill(dt);

            List<Pessoa> pessoas = new List<Pessoa>();

            foreach (DataRow dr in dt.Rows)
            {
                pessoas.Add(new Pessoa
                {
                    Id = dr.Field<int>("ID"),
                    Name = dr.Field<string>("txt_nm"),
                    Email = dr.Field<string>("email")
                });
            }

            return Ok(pessoas);
        }

        [HttpGet]
        [Route("/pessoa/{id}")]
        public IActionResult GetPessoasByID(int id)
        {
            
            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                var query = $"P_SEL_PESSOA_X_ID {id}";
                SqlCommand command = new SqlCommand(query, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                Pessoa pessoa = new Pessoa();

                if (!reader.HasRows) return StatusCode(404, "Pessoa não encontrada");

                while (reader.Read())
                {
                    pessoa.Id = Convert.ToInt32(reader["ID"]);
                    pessoa.Name = reader["txt_nm"].ToString();
                    pessoa.Email = reader["email"].ToString();
                }
                return Ok(pessoa);
            }

        }
        [HttpPost]
        [Route("/pessoa")]
        public IActionResult CreatePerson(Pessoa pessoa)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default")))
                {
                    using (SqlCommand cmd = new SqlCommand("p_ins_pessoa", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = pessoa.Name;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar, 50).Value = pessoa.Email;

                        connection.Open();
                        var newPessoaID = cmd.ExecuteScalar();

                        pessoa.Id = Convert.ToInt32(newPessoaID);
                    }
                }
                return Ok(pessoa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete]
        [Route("/pessoa/{id}")]
        public IActionResult DeletePerson(int id)
        {
            try
            {
                if (id == null) return NotFound();

                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default")))
                {
                    using (SqlCommand command = new SqlCommand("p_del_pessoa", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID", id);
                        connection.Open();
                        int rows = command.ExecuteNonQuery();

                        if(!(rows > 0))
                        {
                            return StatusCode(404, "Pessoa não encontrada ou já deletada");
                        }

                        return StatusCode(204, "Pessoa deletada com sucesso");
                    }
                }

            }
            catch (Exception ex)
            {
                return (StatusCode(500, ex.Message));
            }
        }

        [HttpPut]
        [Route("/pessoa/{id}")]
        public IActionResult UpdatePerson(int id, [FromBody] Pessoa pessoa)
        {
            if (id == null) return StatusCode(404, "Necessário informar um ID");

            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default")))
                {
                    using (SqlCommand command = new SqlCommand("p_upd_pessoa", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID", id);
                        
                        if (pessoa.Name != null)
                        {
                            command.Parameters.AddWithValue("@Name", pessoa.Name);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Name", DBNull.Value);
                        }

                        if (pessoa.Email != null)
                        {
                            command.Parameters.AddWithValue("@Email", pessoa.Email);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@Email", DBNull.Value);
                        }

                        connection.Open();
                        int rows = command.ExecuteNonQuery();

                        if (!(rows > 0))
                        {
                            return StatusCode(404, "Pessoa não encontrada");
                        }

                        return StatusCode(204, "Pessoa atualizada com sucesso");
                    }

                }
            }
            catch (Exception ex)
            {
                return (StatusCode(500, ex.Message));
            }
        }

    }
}
