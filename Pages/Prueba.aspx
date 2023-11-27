<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true" CodeBehind="Prueba.aspx.cs" Inherits="GTProyect.Pages.Prueba" %>
<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">
    Página de prueba
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server">
   
    <form runat="server">
        <div>
            <asp:Literal runat="server" ID="ResultLiteral"></asp:Literal>
        </div>
    </form>
</asp:Content>
