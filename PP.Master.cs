using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace GTProyect
{
    public partial class PP : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Verifica si la pagina es el index para apagar el boton de cerrar sesion
            if (Request.Url.AbsolutePath.EndsWith("Login.aspx"))
            {
                LnkCerrarSesion.Visible = false;
            }
            else
            {
                LnkCerrarSesion.Visible = true;
            }
        }
    }
}