using System;
using System.Collections.Generic;
using System.Configuration; // Asegúrate de importar esta referencia
using System.Data;
using System.Web.UI.WebControls;
using Npgsql;
using NpgsqlTypes;

namespace GTProyect.Pages
{
    public partial class Index : System.Web.UI.Page
    {
        // Utiliza la cadena de conexión desde el archivo de configuración
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["TuConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarPaises();
                CargarTabla();
            }
        }

        void CargarTabla()
        {
            try
            {
                string selectedPais = ddlFiltroPaises.SelectedValue;

                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    con.Open();

                    // Utiliza una consulta directa en lugar de un procedimiento almacenado
                    using (NpgsqlCommand cmd = new NpgsqlCommand($"SELECT kw, activo, nombre_pais FROM \"IA_GTRENDS\".Listarkw('{selectedPais}')", con))
                    {
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            // Cambia el nombre de las columnas según tu preferencia
                            CambiarNombreColumna(dt, "kw", "Keyword");
                            CambiarNombreColumna(dt, "activo", "Activo");
                            CambiarNombreColumna(dt, "nombre_pais", "NombrePais");

                            keywordslist.DataSource = dt;
                            keywordslist.DataBind();

                            // Agregar una opción para "Todos 
                            ddlFiltroPaises.Items.Insert(0, new ListItem("Todos", "Todos"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("Error al cargar la tabla: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Response.Write("Inner Exception: " + ex.InnerException.Message);
                }
            }
        }


        // Método para cambiar el nombre de una columna en un DataTable
        private void CambiarNombreColumna(DataTable dt, string nombreAntiguo, string nombreNuevo)
        {
            if (dt.Columns.Contains(nombreAntiguo))
            {
                dt.Columns[nombreAntiguo].ColumnName = nombreNuevo;
            }
        }

        protected void BtnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Redirige a la página CRUD.aspx
                Response.Redirect("CRUD.aspx");
            }
            catch (Exception ex)
            {
                Response.Write("Error al redirigir: " + ex.Message);
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
                                // Asegúrate de que el nombre de la columna coincida con el resultado de la consulta
                                string nombre_pais = reader["nombre"].ToString();

                                // Verificar si el país ya existe en la lista
                                if (!paises.Contains(nombre_pais))
                                {
                                    paises.Add(nombre_pais);
                                }
                            }

                            // Verificar si "Todos los países" ya existe en la lista
                            if (!paises.Contains("Todos"))
                            {
                                // Verificar si "Todos los países" está seleccionado
                                bool todosSelected = ddlFiltroPaises.SelectedIndex == 0;

                                // Agregar una opción para "Todos los países" solo si no está seleccionado
                                if (!todosSelected)
                                {
                                    paises.Insert(0, "Todos");
                                }
                            }

                            // Asigna la lista de países al DropDownList
                            ddlFiltroPaises.DataSource = paises;
                            ddlFiltroPaises.DataBind();
                        }
                    }
                }
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
                // Obtener el botón que desencadenó el evento
                Button btnModify = (Button)sender;

                // Obtener la fila del GridView que contiene el botón
                GridViewRow row = (GridViewRow)btnModify.NamingContainer;

                // Obtener el valor de la columna "Keyword" en esa fila
                string keyword = ((Label)row.FindControl("LblKeyword")).Text;

                // Redirigir al CRUD con el parámetro "kw" y "modo" en la URL
                Response.Redirect($"CRUD.aspx?kw={keyword}&modo=Modificar");
            }
            catch (Exception ex)
            {
                Response.Write("Error al modificar: " + ex.Message);
            }
        }

        protected void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                Button btnDelete = (Button)sender;
                string[] arguments = btnDelete.CommandArgument.ToString().Split(';');
                string keyword = arguments[0];
                string country = arguments[1];
                // Llamada al procedimiento almacenado DesactivarKeyword
                string mensaje;
                DesactivarKeyword(keyword, country, out mensaje);

                Response.Write(mensaje);

                // Recargar la tabla después de eliminar
                CargarTabla();
            }
            catch (Exception ex)
            {
                Response.Write("Error al eliminar: " + ex.Message);
            }
        }

        private void DesactivarKeyword(string keyword, string country, out string mensaje)
        {
            try
            {
                using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand("DesactivarKeyword", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@kw", keyword);
                        cmd.Parameters.AddWithValue("@nombrePais", country);
                        cmd.Parameters.AddWithValue("@activarParaTodos", 0); 

                        // Parámetro de salida
                        NpgsqlParameter outputParam = new NpgsqlParameter("@mensaje", NpgsqlDbType.Varchar, 100);
                        outputParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParam);

                        con.Open();
                        cmd.ExecuteNonQuery();

                        // Obtener el mensaje de salida
                        mensaje = outputParam.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones
                mensaje = "Error al desactivar keyword: " + ex.Message;
            }
        }

        protected void HyperLinkModify_Click(object sender, EventArgs e)
        {
            // Obtén la keyword seleccionada desde el GridView
            string keyword = ObtenerValorSeleccionadoDesdeGridView();

            // Redirige a la página CRUD con el parámetro "modo" establecido en "Modificar"
            Response.Redirect($"CRUD.aspx?kw={keyword}&modo=Modificar");
        }

        private string ObtenerValorSeleccionadoDesdeGridView()
        {
            foreach (GridViewRow row in keywordslist.Rows)
            {
                RadioButton rb = row.FindControl("RbSelect") as RadioButton;
                if (rb.Checked)
                {
                    return (row.FindControl("LblKeyword") as Label).Text;
                }
            }
            return null;
        }
    }
}