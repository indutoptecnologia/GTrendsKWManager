using System;
using System.Collections.Generic;
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

                // Verificar si se ingresaron todos los datos
                if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(selectedCountry))
                {
                    // Mostrar mensaje en el popup
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "showErrorPopup", "ShowPopup('Error: Debes ingresar tanto la keyword como el país.');", true);
                    return;
                }

                int codp;
              

                // Obtener el código del país desde la tabla pais usando el nombre del país
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    using (NpgsqlCommand cmd = new NpgsqlCommand())
                    {
                        cmd.Connection = con;

                        // Utilizar una consulta SQL para obtener el código del país
                        
                        cmd.CommandText = "SELECT codp FROM \"IA_GTRENDS\".pais WHERE nombre = selectedCountry";
                        selectedCountry = cmd.CommandText;

                        // Ejecutar el comando
                        object result = cmd.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out codp))
                        {
                            // El valor se obtuvo correctamente, ahora puedes usar codp para la inserción

                            // Utilizar una consulta SQL para insertar
                            cmd.Parameters.Clear(); // Limpiar los parámetros antes de usarlos nuevamente
                            cmd.CommandText = "INSERT INTO \"IA_GTRENDS\".keywords (codp, kw, activo) VALUES (@codp, @kw, 1)";

                            // Configurar parámetros...
                            cmd.Parameters.AddWithValue("@kw", keyword);
                            cmd.Parameters.AddWithValue("@codp", codp);

                            // Ejecutar el comando
                            cmd.ExecuteNonQuery();

                            // Mostrar mensaje en el popup
                            string mensaje = modo == "Modificar" ? "Keyword modificada exitosamente." : "Keyword insertada exitosamente.";
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "showResultPopup", $"ShowPopup('{mensaje}');", true);
                        }
                        else
                        {
                            // El valor no se obtuvo correctamente, muestra un mensaje de error
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "showErrorPopup", "ShowPopup('Error: No se pudo obtener el código del país.');", true);
                        }
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



        //// Método para obtener la lista de países disponibles
        //private List<string> ObtenerPaisesDisponibles()
        //{
        //    List<string> paises = new List<string>();


        //    return paises;
        //}

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
