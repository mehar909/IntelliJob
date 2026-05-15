<%@ Page Title="AI Interview" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="Interview.aspx.cs" Inherits="IntelliJob.User.Interview" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        body {
            background: #ffffff;
        }

        .page-header-section {
            background: linear-gradient(100deg, #da2461 10%, #011B43 90%);
            padding: 54px 0 28px;
            min-height: 450px;
        }

            .page-header-section .container {
                padding: 100px 32px;
            }

            .page-header-section h2 {
                font-size: 40px;
                font-weight: 700;
                text-transform: capitalize;
                font-family: "Muli", sans-serif;
                color: #ffffff;
                margin-bottom: 10px;
            }

            .page-header-section p {
                color: #ffffff;
                font-size: 20px;
                max-width: 600px;
            }

        .interview-setup-card {
            background: rgba(255,255,255,0.96);
            border-radius: 24px;
            box-shadow: 0 18px 40px rgba(15, 23, 42, 0.08);
            padding: 40px;
            margin-bottom: 30px;
            border: 1px solid #edf2f7;
            backdrop-filter: blur(10px);
        }

            .interview-setup-card h3 {
                font-size: 26px;
                font-weight: 800;
                color: #111827;
                margin-bottom: 10px;
            }

            .interview-setup-card p.subtitle {
                color: #6b7280;
                margin-bottom: 30px;
                font-size: 15px;
            }

        .form-section-title {
            font-size: 14px;
            font-weight: 600;
            color: #fb246a;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 15px;
            margin-top: 25px;
        }

        .interview-setup-card .form-control {
            border-radius: 12px;
            border: 1px solid #dee2e6;
            padding: 12px 15px;
            font-size: 15px;
            transition: border-color 0.3s ease, box-shadow 0.3s ease;
            background: #fff;
        }

        Make dropdown menus larger and more prominent
        .interview-setup-card select.form-control {
            padding: 14px 18px;
            font-size: 16px;
            font-weight: 500;
            min-height: 50px;
        }

        .interview-setup-card .form-control:focus {
            border-color: #fb246a;
            box-shadow: 0 0 0 0.2rem rgba(255, 67, 87, 0.15);
        }

        .interview-setup-card label {
            font-weight: 600;
            color: #374151;
            margin-bottom: 6px;
            font-size: 14px;
        }

        .btn-start-interview-placeholder {
            display: none;
        }

        .create-mock-wrap {
            display: flex;
            justify-content: center;
            padding: 38px 0 0;
        }

        .btn-create-mock {
            display: inline-flex;
            align-items: center;
            gap: 10px;
            padding: 14px 24px;
            border-radius: 999px;
            border: none;
            background: #8b92dd;
            /*linear-gradient(135deg, #111827 0%, #fb246a 100%);*/
            color: #fff;
            font-weight: 700;
            box-shadow: 0 12px 28px rgba(17, 24, 39, 0.18);
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

            .btn-create-mock:hover {
                color: #fff;
                text-decoration: none;
                transform: translateY(-1px);
                box-shadow: 0 16px 32px rgba(17, 24, 39, 0.2);
            }

        .interview-setup-shell {
            display: none;
            min-width:900;
        }

        .techstack-tags {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            margin-top: 8px;
        }

        .techstack-tag {
            background: #f0f0f0;
            color: #2d3436;
            padding: 6px 14px;
            border-radius: 20px;
            font-size: 13px;
            font-weight: 500;
            display: inline-flex;
            align-items: center;
            gap: 6px;
        }

            .techstack-tag .remove-tag {
                cursor: pointer;
                color: #fb246a;
                font-weight: 700;
                font-size: 16px;
                line-height: 1;
            }

        .history-card {
            background: #ffffff;
            border-radius: 18px;
            box-shadow: 0 10px 28px rgba(15, 23, 42, 0.06);
            padding: 22px 25px;
            border: 1px solid #edf2f7;
            margin-bottom: 15px;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
            overflow: visible;
            min-width: 900px;
            max-width: 960px;
        }

            .history-card:hover {
                transform: translateY(-2px);
                box-shadow: 0 6px 24px rgba(0, 0, 0, 0.12);
            }

            .history-card .row {
                align-items: center;
                flex-wrap: nowrap !important;
                margin: 0 -10px;
            }

                .history-card .row > div {
                    padding: 0 10px;
                }

            .history-card .role-title {
                font-size: 18px;
                font-weight: 700;
                color: #2d3436;
                margin-bottom: 5px;
            }

            .history-card .meta-info {
                color: #636e72;
                font-size: 13px;
            }

            .history-card .score-badge {
                display: inline-block;
                padding: 6px 16px;
                border-radius: 20px;
                font-weight: 700;
                font-size: 14px;
            }

        .score-high {
            background: #d4edda;
            color: #155724;
        }

        .score-mid {
            background: #fff3cd;
            color: #856404;
        }

        .score-low {
            background: #f8d7da;
            color: #721c24;
        }

        .status-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
        }

        .status-pending {
            background: #fff3cd;
            color: #856404;
        }

        .status-completed {
            background: #d4edda;
            color: #155724;
        }

        .status-in-progress {
            background: #cce5ff;
            color: #004085;
        }

        .status-cancelled {
            background: #f8d7da;
            color: #721c24;
        }

        .status-access-revoked {
            background: #f8d7da;
            color: #721c24;
            opacity: 0.8;
        }

        /* Unified card buttons - same size, same style */
        .btn-card-primary {
            border: 1px solid #fb246a;
            color: #fb246a;
            border-radius: 6px;
            padding: 6px 16px;
            font-size: 13px;
            font-weight: 600;
            text-decoration: none;
            transition: all 0.3s ease;
        }

            .btn-card-primary:hover {
               background: #fb246a;
                color: #fff;
                text-decoration: none;
                transform: translateY(-1px);
            }

            .btn-card-primary.disabled {
                opacity: 0.45;
                cursor: not-allowed;
                pointer-events: none;
            }
        /* Retake button — outlined, same red theme */
        .btn-card-outline {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 5px;
            padding: 10px 22px;
            font-size: 14px;
            font-weight: 600;
            border-radius: 12px;
            border: 2px solid #fb246a;
            background-color: transparent;
            color: #fb246a !important;
            white-space: nowrap;
            text-decoration: none;
            transition: all 0.2s ease;
            cursor: pointer;
            line-height: 1.4;
        }

            .btn-card-outline:hover {
                background-color: #fb246a;
                color: #fff !important;
                text-decoration: none;
                transform: translateY(-1px);
            }

        /* View Past Interviews header button */
        .btn-header-action {
            display: inline-flex;
            align-items: center;
            gap: 8px;
            background-color: #fb246a;
            border: 2px solid #fb246a;
            color: #ffffff !important;
            border-radius: 12px;
            padding: 12px 28px;
            font-weight: 600;
            font-size: 15px;
            text-decoration: none;
            transition: all 0.2s ease;
            white-space: nowrap;
        }

            .btn-header-action:hover {
                background-color: #da2461;
                border-color: #da2461;
                color: #fff !important;
                text-decoration: none;
                transform: translateY(-1px);
            }

        /* Generate Interview button */
        .btn-start-interview {
            background: #fb246a !important;
            background-image: none !important;
            color: #fff !important;
            border: none;
            border-radius: 12px;
            padding: 14px 40px;
            font-size: 16px;
            font-weight: 600; /*
            text-transform: uppercase;*/
            letter-spacing: 0.5px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(251, 36, 106, 0.3);
            cursor: pointer;
        }

            .btn-start-interview:hover {
                background: #da2461 !important;
                transform: translateY(-2px);
                box-shadow: 0 6px 16px rgba(251, 36, 106, 0.4);
            }

        /* Form dropdowns — match text input height */
        .interview-setup-card select.form-control {
            height: 50px;
            padding: 12px 15px;
            font-size: 15px;
            appearance: auto;
        }

        .page-header-section {
            padding: 54px 0 28px;
        }

            .page-header-section h2 {
                font-size: 34px;
                font-weight: 800;
                color: #ffffff;
            }

            .page-header-section p {
                color: rgba(255,255,255,0.82);
                font-size: 16px;
            }

        .interview-setup-card .form-group label {
            display: block;
            margin-bottom: 6px;
        }

        .interview-table-card {
            background: inherit;
            overflow: hidden;
            box-shadow:none;
        }


        .history-card .btn-card-primary,
        .history-card .btn-card-outline {
            min-width: 155px;
        }

        @media (max-width: 991.98px) {
            .page-header-section .container {
                padding: 24px;
            }

            .page-header-section h2 {
                font-size: 30px;
            }

            .interview-setup-card {
                padding: 28px 22px;
            }

            .history-card {
                min-width: 100%;
            }
        }

        @media (max-width: 767.98px) {
            .page-header-section .col-lg-4 {
                text-align: left !important;
                padding-top: 18px !important;
            }

            .btn-header-action {
                width: 100%;
                justify-content: center;
            }

            .history-card .col-md-4,
            .history-card .col-md-2,
            .history-card .col-md-4:last-child {
                text-align: left !important;
                margin-bottom: 12px;
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>
        <!-- Page Header -->
        <div class="page-header-section">
            <div class="container">
                <div class="row">
                    <div class="col-lg-8">
                        <h2>AI Interviewer</h2>
                        <p>Practice your interview skills with our AI-powered interviewer. Get instant feedback and improve your performance.</p>
                    </div>
                    <div class="col-lg-4 text-right pt-3">
                        <a href="InterviewHistory.aspx" class="btn-header-action">
                            <i class="fas fa-history"></i>View Past Interviews
                        </a>
                    </div>
                </div>
            </div>
        </div>

        <div class="create-mock-wrap" style="margin-top: 22px; margin-bottom: 10px;">
            <button type="button" class="btn-create-mock" onclick="showInterviewSetup();">
                <i class="fas fa-video"></i>Create Your Own Interview
            </button>
        </div>

        <!-- Interview Setup Form -->
        <section class="pt-0 interview-setup-shell" id="interviewSetupShell" style="padding-bottom: 20px;">
            <div class="container">
                <div class="row justify-content-center">
                    <div class="col-xl-8 col-lg-10">
                        <div class="interview-setup-card">
                            <h3>Setup Your Interview</h3>
                            <p class="subtitle">Configure the interview parameters to get a personalized experience.</p>

                            <asp:Label ID="lblMsg" runat="server" Visible="false"></asp:Label>

                            <div class="form-section-title">Interview Details</div>

                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label for="<%= txtRole.ClientID %>">Job Role / Position</label>
                                        <asp:TextBox ID="txtRole" runat="server" CssClass="form-control" placeholder="e.g., Software Engineer"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="rfvRole" runat="server" ControlToValidate="txtRole"
                                            ErrorMessage="Role is required" CssClass="text-danger" Display="Dynamic" ValidationGroup="interview"></asp:RequiredFieldValidator>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label for="<%= ddlLevel.ClientID %>">Experience Level</label>
                                        <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-control w-100">
                                            <asp:ListItem Text="Select Level" Value="" />
                                            <asp:ListItem Text="Junior (0-2 years)" Value="Junior" />
                                            <asp:ListItem Text="Mid-Level (2-5 years)" Value="Mid" />
                                            <asp:ListItem Text="Senior (5+ years)" Value="Senior" />
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvLevel" runat="server" ForeColor="Red" Display="Dynamic" SetFocusOnError="true" Font-Size="Small" InitialValue="0" ControlToValidate="ddlLevel"
                                            ErrorMessage="Level is required" CssClass="text-danger" ValidationGroup="interview"></asp:RequiredFieldValidator>
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label for="<%= ddlType.ClientID %>">Interview Type</label>
                                        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control w-100">
                                            <asp:ListItem Text="Select Type" Value="" />
                                            <asp:ListItem Text="Technical" Value="Technical" />
                                            <asp:ListItem Text="Behavioral" Value="Behavioral" />
                                            <asp:ListItem Text="Mixed" Value="Mixed" />
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvType" runat="server" ControlToValidate="ddlType"
                                            ErrorMessage="Type is required" CssClass="text-danger" Display="Dynamic" ValidationGroup="interview"></asp:RequiredFieldValidator>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label for="<%= ddlQuestionCount.ClientID %>">Number of Questions</label>
                                        <asp:DropDownList ID="ddlQuestionCount" runat="server" CssClass="form-control w-100">
                                            <asp:ListItem Text="3 Questions" Value="3" />
                                            <asp:ListItem Text="5 Questions" Value="5" Selected="True" />
                                            <asp:ListItem Text="8 Questions" Value="8" />
                                            <asp:ListItem Text="10 Questions" Value="10" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                            </div>

                            <div class="form-section-title">Technology Stack</div>

                            <div class="form-group mb-3">
                                <label>Tech Stack (type and press Enter or comma to add)</label>
                                <input type="text" id="txtTechInput" class="form-control" placeholder="e.g., C#, ASP.NET, SQL Server" />
                                <asp:HiddenField ID="hdnTechStack" runat="server" />
                                <div id="techTagContainer" class="techstack-tags"></div>
                            </div>

                            <div style="margin-top: 35px; display: flex; justify-content: center;">
                                <asp:Button ID="btnStartInterview" runat="server" Text="Generate Interview"
                                    CssClass="btn-start-interview" OnClick="btnStartInterview_Click" ValidationGroup="interview" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- Recent Interviews Section -->
        <section style="padding-bottom: 40px;">
        <div class="container">
            <div class="row justify-content-center mt-5">
    <%--                    <div class="col-xl-10 col-lg-10">--%>
                <div class="interview-table-card">
                    <h4 style="font-weight: 700; color: #2d3436; margin-bottom: 20px;">
                        <i class="fas fa-clock" style="color: #fb246a;"></i>Your Recent Interviews
                    </h4>

                    <asp:Repeater ID="rptRecentInterviews" runat="server">
                        <ItemTemplate>
                            <div class="history-card">
                                <div class="row align-items-center" style="flex-wrap: nowrap;">
                                    <div class="col-md-4">
                                        <div class="role-title"><%# Eval("Role") %> Interview</div>
                                        <div class="meta-info">
                                            <i class="fas fa-layer-group"></i><%# Eval("Level") %> &middot;
                                        <i class="fas fa-tag"></i><%# Eval("InterviewType") %> &middot;
                                        <%# GetTimeAgo(Eval("CreatedAt")) %>
                                        </div>
                                    </div>
                                    <div class="col-md-2 text-center">
                                        <span class='status-badge status-<%# Eval("DisplayStatus").ToString().ToLower().Replace(" ", "-") %>'>
                                            <%# Eval("DisplayStatus").ToString().ToLower() == "access-revoked" ? "Access Revoked" : Eval("Status").ToString() %>
                                        </span>
                                    </div>
                                    <div class="col-md-2 text-center">
                                        <%# GetScoreBadge(Eval("TotalScore")) %>
                                    </div>
                                    <div class="col-md-4 text-center" >
                                        <a href='<%# GetInterviewLink(Eval("InterviewId"), Eval("Status"), Eval("IsCompanyInterview"), Eval("IsPasswordUsed"), Eval("AccessToken")) %>'
                                            class='btn-card-primary <%# (Eval("DisplayStatus").ToString().ToLower() == "access-revoked" || Eval("Status").ToString().ToLower() == "cancelled") ? "disabled" : "" %>'>
                                            <%# GetActionText(Eval("Status"), Eval("IsCompanyInterview"), Eval("IsPasswordUsed")) %>
                                        </a>
                                        <%# GetRetakeButton(Eval("InterviewId"), Eval("Status"), Eval("IsCompanyInterview")) %>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Literal ID="litNoInterviews" runat="server" Visible="false" />
                </div>
            </div>

        </div>
        </section>
    </main>

    <!-- Loading Overlay -->
    <div id="loadingOverlay" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(255,255,255,0.85); z-index: 9999; justify-content: center; align-items: center; flex-direction: column;">
        <div style="text-align: center;">
            <div style="width: 60px; height: 60px; border: 5px solid #f3f3f3; border-top: 5px solid #fb246a; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 20px;"></div>
            <h4 style="color: #2d3436; font-weight: 700;">Generating Your Interview...</h4>
            <p style="color: #636e72;">AI is preparing personalized questions. This may take a few seconds.</p>

        </div>
    </div>
    <style>
        @keyframes spin {
            0% {
                transform: rotate(0deg)
            }

            100% {
                transform: rotate(360deg)
            }
        }
    </style>

    <script type="text/javascript">
        function showInterviewSetup() {
            var shell = document.getElementById('interviewSetupShell');
            if (!shell) return;
            shell.style.display = 'block';
            shell.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }

        // Hide loading overlay when page is fully loaded (after postback)
        window.addEventListener('load', function () {
            var overlay = document.getElementById('loadingOverlay');
            if (overlay) overlay.style.display = 'none';
        });

        // Show loading overlay on form submit
        var form = document.querySelector('form');
        if (form) {
            form.addEventListener('submit', function (e) {
                if (typeof Page_ClientValidate === 'function') {
                    var valid = Page_ClientValidate('interview');
                    if (!valid) return;
                }
                var overlay = document.getElementById('loadingOverlay');
                if (overlay) overlay.style.display = 'flex';
            });
        }

        // Tech stack tag management
        var techTags = [];
        var techInput = document.getElementById('txtTechInput');
        var tagContainer = document.getElementById('techTagContainer');
        var hiddenField = document.getElementById('<%= hdnTechStack.ClientID %>');

        techInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ',') {
                e.preventDefault();
                addTag(this.value.trim());
                this.value = '';
            }
        });

        techInput.addEventListener('blur', function () {
            if (this.value.trim()) {
                addTag(this.value.trim());
                this.value = '';
            }
        });

        function addTag(text) {
            text = text.replace(/,/g, '').trim();
            if (!text || techTags.indexOf(text.toLowerCase()) !== -1) return;

            techTags.push(text.toLowerCase());
            renderTags();
            hiddenField.value = techTags.join(',');
        }

        function removeTag(index) {
            techTags.splice(index, 1);
            renderTags();
            hiddenField.value = techTags.join(',');
        }

        function renderTags() {
            tagContainer.innerHTML = '';
            for (var i = 0; i < techTags.length; i++) {
                var span = document.createElement('span');
                span.className = 'techstack-tag';
                span.innerHTML = techTags[i] + ' <span class="remove-tag" onclick="removeTag(' + i + ')">&times;</span>';
                tagContainer.appendChild(span);
            }
        }

        // Auto-dismiss success/info banner after 4 seconds
        (function () {
            var msg = document.getElementById('<%= lblMsg.ClientID %>');
            if (msg && msg.offsetParent !== null && msg.innerHTML.trim() !== '') {
                setTimeout(function () {
                    msg.style.transition = 'opacity 0.5s ease';
                    msg.style.opacity = '0';
                    setTimeout(function () { msg.style.display = 'none'; }, 500);
                }, 4000);
            }
        })();

        function retakeInterview(interviewId) {
            if (!confirm('Retake this interview with the same settings?')) return;
            var btn = event.currentTarget;
            btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creating...';
            btn.style.pointerEvents = 'none';
            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'RetakeInterview.ashx', true);
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        try {
                            var resp = JSON.parse(xhr.responseText);
                            if (resp.success && resp.newInterviewId) {
                                window.location.href = 'Interview.aspx';
                                return;
                            }
                        } catch (e) { }
                        alert('Failed to create retake interview.');
                    } else {
                        alert('Failed to retake: ' + xhr.responseText);
                    }
                    btn.innerHTML = '<i class="fas fa-redo"></i> Retake';
                    btn.style.pointerEvents = '';
                }
            };
            xhr.send('InterviewId=' + encodeURIComponent(interviewId));
        }
    </script>
</asp:Content>
