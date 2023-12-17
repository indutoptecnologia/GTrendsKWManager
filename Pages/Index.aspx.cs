using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Npgsql;
using NpgsqlTypes;

namespace GTProyect.Pages
{
    public partial class Index : System.Web.UI.Page
    {
        // Usa la cadena de conexion
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (UsuarioAutenticado())
                {
                    CargarPaises();
                    CargarTabla();
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }
       

        private bool UsuarioAutenticado()
{
    // Verificar si la variable de sesión "UsuarioAutenticado" está presente y es verdadera
    return Session["UsuarioAutenticado"] != null && (bool)Session["UsuarioAutenticado"];
}

       
        private void CambiarNombreColumna(DataTable dt, string nombreAntiguo, string nombreNuevo)
        {
            if (dt.Columns.Contains(nombreAntiguo))
            {
                dt.Columns[nombreAntiguo].ColumnName = nombreNuevo;
            }
        }

        protected void ddlFiltroStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarTabla();
        }

        protected void BtnAdd_Click(object sender, EventArgs e)
        {
            try

            {
                // Limpio estas variables de sesion en caso de agregar
                Session["SelectedKeyword"] = null;
                Session["SelectedName"] = null;
                Session["SelectedCountry"] = null;


                // Voy al crud
                Response.Redirect("CRUD.aspx");
            }
            catch (Exception ex)
            {
                Response.Write("Error al redirigir: " + ex.Message);
            }
        }
        void CargarTabla()
        {
            try
            {
                string selectedPais = ddlFiltroPaises.SelectedValue;
                string selectedStatus = ddlFiltroStatus.SelectedValue;

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    string sql = "SELECT k.kw, k.activo, p.nombre FROM \"IA_GTRENDS\".keywords k INNER JOIN \"IA_GTRENDS\".pais p ON k.codp = p.codp WHERE (@selectedPais = 'Todos' OR p.nombre = @selectedPais) AND " +
             "(@status = 'Todos' OR (k.activo = true AND @status = 'Activo') OR (k.activo = false AND @status = 'NoActivo'))ORDER BY k.kw";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@selectedPais", selectedPais);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // Cambio nombre de columnas
                            CambiarNombreColumna(dt, "kw", "Palabra clave");
                            CambiarNombreColumna(dt, "activo", "Status");
                            CambiarNombreColumna(dt, "nombre", "Pais");

                            keywordslist.DataSource = dt;
                            keywordslist.DataBind();
                        }
                    }

                    // Esto es para evitar que se me agreguen en bucle el Todos. Lo elimino si existe
                    if (ddlFiltroPaises.Items.FindByValue("Todos") != null)
                    {
                        ddlFiltroPaises.Items.Remove(ddlFiltroPaises.Items.FindByValue("Todos"));
                    }

                    // Aqui agrego la opcion todos
                    ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
                }
            }
            catch (NpgsqlException npgEx)
            {
                // Manejo de errores especificos
                Response.Write("Error Npgsql: " + npgEx.Message);
                Response.Write("<br />Stack Trace: " + npgEx.StackTrace);
            }
            catch (Exception ex)
            {
                // Manejo general de errores
                Response.Write("Error general: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("<br />Inner Exception: " + ex.InnerException.Message);
                }
            }
        }



        void CargarPaises()
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT nombre FROM \"IA_GTRENDS\".pais", con))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<string> paises = new List<string>();

                            while (reader.Read())
                            {
                                // Revisa que las columnas se llamen igual
                                string nombre_pais = reader["nombre"].ToString();

                                // Verifica si el país ya existe en la lista
                                if (!paises.Contains(nombre_pais))
                                {
                                    paises.Add(nombre_pais);
                                }
                            }

                            // Elimina "Todos" si ya existe
                            if (paises.Contains("Todos"))
                            {
                                paises.Remove("Todos");
                            }

                            // Asigna la lista de países al DropDownList
                            ddlFiltroPaises.DataSource = paises;
                            ddlFiltroPaises.DataBind();
                        }
                    }
                }

                // Agrega una opción para "Todos" después de asignar la fuente de datos al DropDownList
                ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
            }
            catch (Exception ex)
            {
                Response.Write("Error al cargar países: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("Inner Exception: " + ex.InnerException.Message);
                }
            }
        }

        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Obtiene el valor actual de la columna "Status"
                bool status = Convert.ToBoolean(DataBinder.Eval(e.Row.DataItem, "Status"));

                // Obtiene el control CheckBox en la fila
                CheckBox checkBoxStatus = (CheckBox)e.Row.FindControl("CheckBoxStatus");

                // Configura el estado del CheckBox según el valor de "Status"
                checkBoxStatus.Checked = status;
            }
        }
        void CargarTabla(string searchText)
        {
            try
            {
                string selectedPais = ddlFiltroPaises.SelectedValue;
                string selectedStatus = ddlFiltroStatus.SelectedValue;

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();
                    string sql = "SELECT k.kw, k.activo, p.nombre FROM \"IA_GTRENDS\".keywords k INNER JOIN \"IA_GTRENDS\".pais p ON k.codp = p.codp WHERE " +
      "(@searchText IS NULL OR k.kw ILIKE @searchText) AND " +
      "(@selectedPais = 'Todos' OR p.nombre = @selectedPais) AND " +
      "(@status = 'Todos' OR (k.activo = true AND @status = 'Activo') OR (k.activo = false AND @status = 'NoActivo')) ORDER BY k.kw";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@searchText", string.IsNullOrEmpty(searchText) ? (object)DBNull.Value : $"%{searchText}%");
                        cmd.Parameters.AddWithValue("@selectedPais", selectedPais);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                    }


                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@selectedPais", selectedPais);
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                        // Agregar parámetro de texto de búsqueda
                        cmd.Parameters.AddWithValue("@searchText", $"%{searchText}%");

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // Cambio nombre de columnas
                            CambiarNombreColumna(dt, "kw", "Palabra clave");
                            CambiarNombreColumna(dt, "activo", "Status");
                            CambiarNombreColumna(dt, "nombre", "Pais");

                            keywordslist.DataSource = dt;
                            keywordslist.DataBind();
                        }
                    }

                    // Esto es para evitar que se me agreguen en bucle el Todos. Lo elimino si existe
                    if (ddlFiltroPaises.Items.FindByValue("Todos") != null)
                    {
                        ddlFiltroPaises.Items.Remove(ddlFiltroPaises.Items.FindByValue("Todos"));
                    }

                    // Aquí agrego la opción Todos
                    ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
                }
            }
            catch (NpgsqlException npgEx)
            {
                // Manejo de errores específicos
                Response.Write("Error Npgsql: " + npgEx.Message);
                Response.Write("<br />Stack Trace: " + npgEx.StackTrace);
            }
            catch (Exception ex)
            {
                // Manejo general de errores
                Response.Write("Error general: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("<br />Inner Exception: " + ex.InnerException.Message);
                }
            }
        }


        protected void ddlFiltroPaises_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedPais = ddlFiltroPaises.SelectedValue;
            Session["SelectedPais"] = selectedPais;
            CargarTabla();
        }

        protected void BtnModify_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtiene el botón que desencadenó el evento
                Button btnModify = (Button)sender;

                // Obtiene la fila del GridView que contiene el botón
                GridViewRow row = (GridViewRow)btnModify.NamingContainer;

                // Verifica si la fila es nula
                if (row != null)
                {
                    // Obtiene el índice de la fila
                    int rowIndex = row.RowIndex;

                    // Obtiene los valores de las celdas en esa fila
                    string keyword = keywordslist.Rows[rowIndex].Cells[0].Text;
                    string name = keywordslist.Rows[rowIndex].Cells[1].Text;
                    string country = keywordslist.Rows[rowIndex].Cells[2].Text;

                    // Guardr en variables de sesión
                    Session["SelectedKeyword"] = keyword;
                    Session["SelectedName"] = name;
                    Session["SelectedCountry"] = country; 

                    // Redirigir al CRUD con los parametros
                    Response.Redirect("CRUD.aspx");
                }
            }
            catch (Exception ex)
            {
                // Maneja excepciones
                Response.Write("Error al modificar/redirigir: " + ex.Message);
            }
        }

             protected void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtiene la fila seleccionada en el GridView
                Button btnDelete = (Button)sender;
                GridViewRow row = (GridViewRow)btnDelete.NamingContainer;

                // Verifica si la fila es nula
                if (row != null)
                {
                    int rowI = row.RowIndex;
                    // Obtiene valores de las celdas en esa fila
                    string keyword = HttpUtility.HtmlDecode(keywordslist.Rows[rowI].Cells[0].Text);
                    string name = HttpUtility.HtmlDecode(keywordslist.Rows[rowI].Cells[1].Text);
                    string country = HttpUtility.HtmlDecode(keywordslist.Rows[rowI].Cells[2].Text);

                    string codp = ObtenerCodigoPais(country);

                    Debug.WriteLine($"keyword: {keyword}");
                    Debug.WriteLine($"codp: {codp}");

                    // Usa la conexión y comando dentro de bloques using para liberar recursos
                    using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                    {
                        con.Open();
                        string storedProcedure = "\"IA_GTRENDS\".cambiarstatus";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(storedProcedure, con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            // Configura parámetros
                            cmd.Parameters.AddWithValue("@p_codp", codp);
                            cmd.Parameters.AddWithValue("@p_kw", keyword);

                            // Parámetro de salida
                            NpgsqlParameter outputParameter = new NpgsqlParameter("@p_resultado", NpgsqlDbType.Integer)
                            {
                                Direction = ParameterDirection.Output
                            };
                            cmd.Parameters.Add(outputParameter);

                            // Ejecuta el procedimiento almacenado
                            cmd.ExecuteNonQuery();

                            // Obtiene el resultado del parámetro de salida
                            int resultado = Convert.ToInt32(outputParameter.Value);

                            // Verifica el resultado
                            if (resultado == -1)
                            {
                                // Muestra un mensaje o realiza alguna acción adicional si es necesario
                                Debug.WriteLine("Estado de la keyword actualizado exitosamente.");
                                CargarTabla();
                            }
                            else if (resultado == -2)
                            {
                                Debug.WriteLine("No se encontró la keyword.");
                                MostrarMensajeError("No se encontró la keyword.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Maneja cualquier excepción
                Debug.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                // Registra el error y muestra un mensaje al usuario
                RegistrarErrorEnLog(ex);
                MostrarMensajeError($"Error: {ex.Message}");
            }
        }




        // Función para mostrar mensajes de error en el cliente
        private void MostrarMensajeError(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowErrorPopup", $"ShowErrorPopup('{mensaje}')", true);
        }

        
        private void RegistrarErrorEnLog(Exception ex)
        {
            
            Console.WriteLine($"Error Log: {ex.Message}");

           
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }


        // Obtengo codp segun nombre de pais
        private string ObtenerCodigoPais(string country)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT codp FROM \"IA_GTRENDS\".pais WHERE nombre = @Nombre";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombre", country);
    
                    object result = command.ExecuteScalar();
                    return result != null ? result.ToString() : string.Empty;
                }
            }
        }

        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener el texto de búsqueda
                string searchText = tbSearch.Text.Trim();

                // Llamada a la función CargarTabla con el filtro de búsqueda
                CargarTabla(searchText);
            }
            catch (Exception ex)
            { }



        }


    }
}


