using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        public async Task<IActionResult> GetPessoas()
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
        [Route("pessoa/{id}")]
        public async Task<IActionResult> GetPessoasByID(int id)
        {
            using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                var query = $"P_SEL_PESSOA_X_ID {id}";
                SqlCommand command = new SqlCommand(query, connection);
                command.Connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                Pessoa pessoa = new Pessoa();

                if (!reader.HasRows) return null;

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
        public IActionResult CreatePerson(Pessoa pessoa)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_config.GetConnectionString("Default")))
                {
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        var query = $"p_ins_pessoa";
                        cmd.CommandText = query;
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
                return BadRequest(ex.Message);
            }
        }

    }
}
