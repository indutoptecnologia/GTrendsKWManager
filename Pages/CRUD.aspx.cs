using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;

namespace GTProyect.Pages
{
    public partial class CRUD : System.Web.UI.Page
    {
        string connectionString = "tu_cadena_de_conexion"; // Reemplaza con tu cadena de conexión

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPaises();
            }
        }

        protected void BtnConfirm_Click(object sender, EventArgs e)
        {
            // Obtener el valor del keyword ingresado por el usuario
            string keyword = tbkeyword.Text;

            // Obtener el nombre del país seleccionado del DropDownList
            string selectedCountry = ddlCountry.SelectedValue;

            // Realizar la consulta para obtener el codp del país
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT codp FROM pais WHERE nombre = @Nombre";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", selectedCountry);
                    string codp = (string)command.ExecuteScalar();

                    // Insertar un nuevo dato en la tabla keywords
                    string insertQuery = "INSERT INTO keywords (codp, kw, activo) VALUES (@Codp, @Keyword, true)";
                    using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Codp", codp);
                        insertCommand.Parameters.AddWithValue("@Keyword", keyword);
                        insertCommand.ExecuteNonQuery();
                    }
                }
            }

            // Muestra el mensaje en un modal
            string message = "La kw fue agregada con éxito";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowPopup", $"ShowPopup('{message}')", true);
        }

        protected void CargarPaises()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Utilizar una consulta SQL para obtener los nombres de los países
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais", con))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> paises = new List<string>();

                            while (reader.Read())
                            {
                                // Revisar que las columnas se llamen igual
                                string nombre_pais = reader["nombre"].ToString();

                                // Verificar si el país ya existe en la lista
                                if (!paises.Contains(nombre_pais))
                                {
                                    paises.Add(nombre_pais);
                                }
                            }

                            // Eliminar "Todos" si ya existe
                            if (paises.Contains("Todos"))
                            {
                                paises.Remove("Todos");
                            }

                            // Asigna la lista de países al DropDownList
                            ddlCountry.DataSource = paises;
                            ddlCountry.DataBind();
                        }
                    }
                }

                // Agregar una opción para "Todos" después de asignar la fuente de datos al DropDownList
                ddlCountry.Items.Insert(0, new ListItem("Todos", "Todos"));
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error al cargar países: " + ex.Message;
                if (ex.InnerException != null)
                {
                    lblMessage.Text += "<br />Inner Exception: " + ex.InnerException.Message;
                }
            }
        }
    }
}



