using System;
using Npgsql;
using System.Web.UI;
using System.Configuration;  // Agrega esta referencia

namespace GTProyect.Pages
{
    public partial class Prueba : Page
    {
        // Reemplaza "TuConnectionString" con tu cadena de conexión real
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["TuConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Ejecuta la consulta y muestra el resultado
                ExecuteQuery();
            }
        }

        private void ExecuteQuery()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais LIMIT 1", con))
                    {
                        con.Open();
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string primerNombre = reader["nombre"].ToString();
                                ResultLiteral.Text = $"El primer nombre en la tabla pais es: {primerNombre}";
                            }
                            else
                            {
                                ResultLiteral.Text = "La tabla pais está vacía.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones, puedes imprimir el mensaje en la consola o realizar otra acción
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
