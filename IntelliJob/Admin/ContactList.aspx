<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/AdminMaster.Master" AutoEventWireup="true" CodeBehind="ContactList.aspx.cs" Inherits="IntelliJob.Admin.ContactList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

     <div style="background-image: url('../Images/bg.jpg'); width: 100%; height: 720px; background-repeat: no-repeat; background-size: cover; background-attachment: fixed;">
     <div class="container-fluid pt-4 pb-4">
 <div>
     <asp:Label ID="lblMsg" runat="server"></asp:Label>
 </div>

 <div class="row">
     <div class="col-12 pb-3">
         <h3 class="text-center">Contact List/Details</h3>
     </div>
 </div>

 <div class="row mb-3 pt-sm-3">
     <div class="col-md-12">
         <asp:GridView ID="GridView1" runat="server" CssClass="table table-hover table-bordered" HeaderStyle-HorizontalAlign="Center"
             EmptyDataText="No Record to Display..!" AllowPaging="True" PageSize="5"
             OnpageIndexChanging="GridView1_PageIndexChanging" DataKeyNames="ContactId" OnRowDeleting="GridView1_RowDeleting" AutoGenerateColumns="False">
             <Columns>
                 
                 <asp:BoundField DataField="Sr.No" HeaderText="Sr.No">
                 <ItemStyle HorizontalAlign="Center" />
                 </asp:BoundField>

                 <asp:BoundField DataField="Name" HeaderText="User Name">
                 <ItemStyle HorizontalAlign="Center" />
                 </asp:BoundField>

                 <asp:BoundField DataField="Email" HeaderText="Email">
                 <ItemStyle HorizontalAlign="Center" />
                 </asp:BoundField>

                 <asp:BoundField DataField="Subject" HeaderText="Subject">
                 <ItemStyle HorizontalAlign="Center" />
                 </asp:BoundField>

                 <asp:BoundField DataField="Message" HeaderText="Message">
                 <ItemStyle HorizontalAlign="Center" />
                 </asp:BoundField>

                
                 <asp:CommandField CausesValidation="false" HeaderText="Delete" ShowDeleteButton="true"
                   DeleteImageUrl="../assets/img/icon/trashIcon.jpg" ButtonType="Image" >
                     <ControlStyle Height="25px" Width="25px" />
                     <ItemStyle HorizontalAlign="Center" />
                     </asp:CommandField>
                 
             </Columns>
             
         </asp:GridView>
     </div>
 </div>

 
  </div>
   </div>

</asp:Content>
