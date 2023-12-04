<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true"  CodeBehind="CRUD.aspx.cs" Inherits="GTProyect.Pages.CRUD" %>
<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">
    GT Keyword Manager
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="Style.css" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server" onload="OnLoad()">
    <br />
    <div class="mx-auto" style="width: 250px">
        <asp:Label runat="server" CssClass="h2" ID="lbltitulo"></asp:Label>
    </div>
    <div class="d-block mx-auto" style="max-width: 400px; width: 100%;">
        <div class="mb-3">
            <label class="form-label">Palabra clave</label>
            <asp:TextBox runat="server" CssClass="form-control" ID="tbkeyword"></asp:TextBox>
        </div>
        <div class="mb-3">
            <label class="form-label">País</label>
            <asp:DropDownList runat="server" CssClass="form-control" ID="ddlCountry"></asp:DropDownList>
        </div>

        <asp:Button runat="server" CssClass="btn btn-primary btn-dif" ID="BtnCancel" Text="Cancelar" Visible="true"
            PostBackUrl="~/Pages/Index.aspx" />

        <asp:Button runat="server" CssClass="btn btn-primary btn-dif" ID="BtnConfirm" Text="Confirmar" Visible="true" OnClientClick="return ConfirmClick();" OnClick="BtnConfirm_Click" />
        <asp:Button runat="server" CssClass="btn btn-primary btn-dif" ID="BtnModify" Text="Modificar" Visible="true" OnClientClick="return ModifyClick();" OnClick="BtnModify_Click" />

        
        <asp:Label runat="server" ID="lblMessage" ForeColor="Red"></asp:Label>

        <asp:HiddenField runat="server" ID="hdnKeyword" />
        <asp:HiddenField runat="server" ID="hdnSelectedCountry" />
        
        <div class="modal fade" id="myModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="exampleModalLabel">Mensaje</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                       
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                    </div>
                </div>
            </div>
        </div>

        <script src="https://code.jquery.com/jquery-3.6.4.min.js"></script>
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
        <script type="text/javascript">
            function OnLoad() {
                
                var hasSession = '<%= Session["SelectedKeyword"] != null %>';          
                if (hasSession) {
                    
                    $('#<%= BtnConfirm.ClientID %>').prop('disabled', true);
                    $('#<%= BtnModify.ClientID %>').prop('disabled', false);
                } else {
                    
                    $('#<%= BtnConfirm.ClientID %>').prop('disabled', false);
                    $('#<%= BtnModify.ClientID %>').prop('disabled', true);
                }
            }

            function ConfirmClick() {
                
                var keyword = $('#<%= tbkeyword.ClientID %>').val();

               
                var selectedCountry = $('#<%= ddlCountry.ClientID %> option:selected').text();

                $('#<%= hdnKeyword.ClientID %>').val(keyword);
                $('#<%= hdnSelectedCountry.ClientID %>').val(selectedCountry);

                
                __doPostBack('<%= BtnConfirm.UniqueID %>', '');
                return false; 
            }

            function ModifyClick() {
                
                var keyword = $('#<%= tbkeyword.ClientID %>').val();

               
                var selectedCountry = $('#<%= ddlCountry.ClientID %> option:selected').text();
                
                $('#<%= hdnKeyword.ClientID %>').val(keyword);
                $('#<%= hdnSelectedCountry.ClientID %>').val(selectedCountry);
              
                __doPostBack('<%= BtnModify.UniqueID %>', '');
                return false; 
            }
        </script>
    </div>
</asp:Content>


