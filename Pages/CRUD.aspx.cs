using System;
using System.Data;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using Npgsql;

namespace GTProyect.Pages
{
    public partial class CRUD : System.Web.UI.Page
    {
        // Asegúrate de que esta variable contenga la cadena de conexión correcta
        string connectionString = WebConfigurationManager.ConnectionStrings["TuConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPaises();

                // Verifica si el parámetro kw está presente en la URL
                if (Request.QueryString["kw"] != null)
                {
                    // Recupera el valor del parámetro kw
                    string keyword = Request.QueryString["kw"];

                    // Carga los datos de la keyword en el formulario para su modificación
                    CargarDatosKeyword(keyword);
                }
            }
        }

        protected void BtnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener el valor del parámetro "modo" de la URL
                string modo = Request.QueryString["modo"];

                // Obtener los valores de los controles
                string keyword = tbkeyword.Text;
                string selectedCountry = ddlCountry.SelectedValue;
                bool activateGlobally = cbStatusGlobally.Checked;

                // Verificar si se ingresaron todos los datos
                if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(selectedCountry))
                {
                    // Mostrar mensaje en el popup
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "showErrorPopup", "ShowPopup('Error: Debes ingresar tanto la keyword como el país.');", true);
                    return;
                }

                // Crear conexión y comando
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    using (NpgsqlCommand cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = con;

                        if (modo == "Modificar")
                        {
                            // Utilizar una consulta SQL para modificar
                            cmd.CommandText = "UPDATE \"IA_GTRENDS\".keywords SET nombre_pais = @nombrePais, activo = @activarGlobally WHERE kw = @kw";
                        }
                        else
                        {
                            // Utilizar una consulta SQL para insertar
                            cmd.CommandText = "INSERT INTO \"IA_GTRENDS\".keywords (kw, nombre_pais, activo) VALUES (@kw, @nombrePais, @activarParaTodos)";
                        }

                        // Configurar parámetros...
                        cmd.Parameters.AddWithValue("@kw", keyword);
                        cmd.Parameters.AddWithValue("@nombrePais", selectedCountry);
                        cmd.Parameters.AddWithValue("@activarGlobally", activateGlobally);
                        cmd.Parameters.AddWithValue("@activarParaTodos", activateGlobally ? 1 : 0);

                        // Ejecutar el comando
                        cmd.ExecuteNonQuery();

                        // Mostrar mensaje en el popup
                        string mensaje = modo == "Modificar" ? "Keyword modificada exitosamente." : "Keyword insertada exitosamente.";
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "showResultPopup", $"ShowPopup('{mensaje}');", true);
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                // Mostrar mensaje de error en el popup
                ScriptManager.RegisterStartupScript(this, this.GetType(), "showErrorPopup", $"ShowPopup('Error: {ex.Message}');", true);
            }
        }

        private void CargarDatosKeyword(string keyword)
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Utiliza una consulta SQL para obtener los detalles de la keyword
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"SELECT * FROM \"IA_GTRENDS\".keywords WHERE kw = @kw", con))
                    {
                        cmd.Parameters.AddWithValue("@kw", keyword);

                        // Utiliza un lector de datos para obtener los detalles de la keyword
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Asigna los valores a los controles en el formulario
                                tbkeyword.Text = reader["kw"].ToString();
                                ddlCountry.SelectedValue = reader["nombre_pais"].ToString();
                                cbStatusGlobally.Checked = Convert.ToBoolean(reader["activo"]);
                            }
                            else
                            {
                                // La keyword no se encontró, muestra un mensaje o toma medidas según sea necesario
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                // Puedes mostrar un mensaje de error o tomar medidas según sea necesario
            }
        }

        void CargarPaises()
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
                            while (reader.Read())
                            {
                                // Asegúrate de que el nombre de la columna coincida con el resultado de la consulta
                                string nombre_pais = reader["nombre"].ToString();

                                // Verificar si el país ya existe en la lista
                                if (!ddlCountry.Items.Contains(new ListItem(nombre_pais)))
                                {
                                    ddlCountry.Items.Add(nombre_pais);
                                }
                            }

                            // Verificar si "Todos los países" ya existe en la lista
                            if (!ddlCountry.Items.Contains(new ListItem("Todos los países", "Todos")))
                            {
                                // Agregar una opción para "Todos los países"
                                ddlCountry.Items.Insert(0, new ListItem("Todos los países", "Todos"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error general al cargar países: " + ex.Message;
            }
        }
    }
}
