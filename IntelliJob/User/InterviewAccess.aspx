<%@ Page Title="Interview Access" Language="C#" MasterPageFile="~/User/UserMaster.Master"
    AutoEventWireup="true" CodeBehind="InterviewAccess.aspx.cs"
    Inherits="IntelliJob.User.InterviewAccess" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .access-card {
            max-width: 460px;
            margin: 80px auto;
            background: #fff;
            border-radius: 16px;
            box-shadow: 0 8px 32px rgba(0,0,0,.12);
            padding: 48px 40px;
            text-align: center;
        }
        .access-card .icon-wrap {
            width: 72px; height: 72px;
            background: linear-gradient(135deg,#6c5ce7,#a29bfe);
            border-radius: 50%;
            display: flex; align-items: center; justify-content: center;
            margin: 0 auto 24px;
        }
        .access-card h2 { font-size: 22px; font-weight: 700; color: #2d3436; margin-bottom: 6px; }
        .access-card p  { color: #636e72; font-size: 14px; margin-bottom: 28px; }
        .access-card .form-control {
            border-radius: 10px; padding: 12px 16px;
            font-size: 16px; letter-spacing: 2px; text-align: center;
            border: 2px solid #e0e0e0;
        }
        .access-card .form-control:focus { border-color: #6c5ce7; box-shadow: none; }
        .btn-enter {
            background: linear-gradient(135deg,#6c5ce7,#a29bfe);
            color: #fff; border: none; border-radius: 10px;
            padding: 12px 0; font-size: 16px; font-weight: 600;
            width: 100%; margin-top: 16px; cursor: pointer;
            transition: opacity .2s;
        }
        .btn-enter:hover { opacity: .9; }
        .info-box {
            background: #f8f9fa; border-radius: 10px;
            padding: 14px 16px; margin-top: 24px; text-align: left;
        }
        .info-box li { font-size: 13px; color: #636e72; margin-bottom: 4px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="access-card">

        <div class="icon-wrap">
            <i class="fas fa-lock" style="font-size:28px;color:#fff;"></i>
        </div>

        <h2>Interview Room</h2>
        <p>Enter the one-time password from your invitation email to begin your interview.</p>

        <asp:HiddenField ID="hdnJobId" runat="server" />

        <asp:Label ID="lblError" runat="server" Visible="false"
            CssClass="alert alert-danger d-block mb-3" EnableViewState="false" />

        <asp:TextBox ID="txtPassword" runat="server"
            CssClass="form-control"
            placeholder="Enter Interview Password"
            MaxLength="20" autocomplete="off" />

        <asp:Button ID="btnEnter" runat="server" Text="Start Interview"
            CssClass="btn-enter" OnClick="btnEnter_Click" />

        <div class="info-box">
            <ul style="padding-left:16px; margin:0;">
                <li>Each password can only be used once.</li>
                <li>Make sure you are in a quiet environment before starting.</li>
                <li>You will be speaking with an AI voice interviewer.</li>
                <li>The interview will be evaluated automatically after completion.</li>
            </ul>
        </div>

    </div>

</asp:Content>
