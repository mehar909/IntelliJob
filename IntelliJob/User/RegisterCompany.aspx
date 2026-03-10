<%@ Page Title="" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="RegisterCompany.aspx.cs" Inherits="IntelliJob.User.RegisterCompany" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <section>
        <div class="container pt-50 pb-40">
            <div class="row">
                <div class="col-12 pb-20">
                    <asp:Label ID="lblMsg" runat="server" Visible="false"></asp:Label>
                </div>
                <div class="col-12">
                    <asp:LinkButton ID="lbRegisterSwitch" runat="server" CssClass="btn head-btn1" CausesValidation="false" OnClick="lbRegisterSwitch_Click"></asp:LinkButton>
                </div>
                <div class="col-12">
                    <h2 class="contact-title text-center">Company Registration</h2>
                </div>
                <div class="col-lg-6 mx-auto">
                    <div class="form-contact contact_form">
                        <div class="row">
                            <div class="col-12">
                                <h6>Login Information</h6>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Username</label>
                                    <asp:TextBox ID="txtUserName" runat="server" CssClass="form-control" placeholder="Enter Unique Username" required></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>Password</label>
                                    <asp:TextBox ID="txtCompanyPassword" runat="server" CssClass="form-control" placeholder="Enter Company Password" TextMode="Password" required></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-sm-6">
                                <div class="form-group">
                                    <label>Confirm Password</label>
                                    <asp:TextBox ID="txtConfirmCompanyPassword" runat="server" CssClass="form-control" placeholder="Enter Confirm Password" TextMode="Password" required></asp:TextBox>
                                    <asp:CompareValidator ID="CompareValidator1" runat="server" ErrorMessage="Password & Confirm Password should be same." ControlToCompare="txtCompanyPassword" ControlToValidate="txtConfirmCompanyPassword" ForeColor="Red" Display="Dynamic" SetFocusOnError="true" Font-Size="Small"></asp:CompareValidator>
                                </div>
                            </div>
                            <div class="col-12">
                                <h6>Company Information</h6>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Company Name</label>
                                    <asp:TextBox ID="txtCompanyName" runat="server" CssClass="form-control" placeholder="Enter Company Name" required></asp:TextBox>
                                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="Company Name must be in characters" ForeColor="Red" Display="Dynamic" SetFocusOnError="true" Font-Size="Small" ValidationExpression="^[a-zA-Z\s]+$" ControlToValidate="txtCompanyName"></asp:RegularExpressionValidator>
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Address</label>
                                    <asp:TextBox ID="txtAddress" runat="server" CssClass="form-control" placeholder="Enter Company Address/Location" TextMode="MultiLine" required></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Email</label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" placeholder="Enter Email" required TextMode="Email"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Website</label>
                                    <asp:TextBox ID="txtWebsite" runat="server" CssClass="form-control" placeholder="Enter Website Link" required ></asp:TextBox>
                                </div>
                            </div>
                             <div class="col-12">
                                 <div class="form-group">
                                     <label>Description</label>
                                     <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" placeholder="Company Description" required></asp:TextBox>
                                 </div>
                             </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Company Size (Number of Employees)</label>
                                    <asp:TextBox ID="txtCompanySize" runat="server" CssClass="form-control"
                                        placeholder="Enter company size" required></asp:TextBox>
                                    <asp:RegularExpressionValidator 
                                        ID="revCompanySize" 
                                        runat="server"
                                        ControlToValidate="txtCompanySize"
                                        ValidationExpression="^[0-9]+$"
                                        ErrorMessage="Company size must be a valid number."
                                        Display="Dynamic"
                                        ForeColor="Red">
                                    </asp:RegularExpressionValidator>

                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Company Logo</label>
                                    <asp:FileUpload ID="fuimage" runat="server" enctype="multipart/form-data"
                                        CssClass="form-control" ToolTip=".png, .jpg, .jpeg extension only" />
                                </div>
                            </div>
                            <div class="col-12">
                                <div class="form-group">
                                    <label>Country</label>
                                    <asp:DropDownList ID="ddlCountry" runat="server" DataSourceID="SqlDataSource1" CssClass="form-control w-100" AppendDataBoundItems="true" DataTextField="CountryName" DataValueField="CountryName">
                                        <asp:ListItem Value="0">Select Country</asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="Country is required" ForeColor="Red" Display="Dynamic" SetFocusOnError="true" Font-Size="Small" InitialValue="0" ControlToValidate="ddlCountry"></asp:RequiredFieldValidator>
                                    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:cs %>" SelectCommand="SELECT CountryName FROM Country"></asp:SqlDataSource>
                                </div>
                            </div>                           
                        </div>
                        <div class="form-group mt-3">
                            <asp:Button ID="btnRegisterCompany" runat="server" Text="Register" CssClass="button button-contactForm boxed-btn" OnClick="btnRegister_Click" />
                            <span class="clickLink">
                                <a href="../User/Login.aspx">Already Registered Your Company? Click Here..
                                </a>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>
</asp:Content>
