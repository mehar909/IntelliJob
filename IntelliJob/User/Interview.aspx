<%@ Page Title="AI Interview" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="Interview.aspx.cs" Inherits="IntelliJob.User.Interview" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .interview-setup-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            padding: 40px;
            margin-bottom: 30px;
            border: 1px solid #e9ecef;
        }

        .interview-setup-card h3 {
            font-size: 24px;
            font-weight: 700;
            color: #2d3436;
            margin-bottom: 10px;
        }

        .interview-setup-card p.subtitle {
            color: #636e72;
            margin-bottom: 30px;
            font-size: 15px;
        }

        .form-section-title {
            font-size: 14px;
            font-weight: 600;
            color: #FF4357;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 15px;
            margin-top: 25px;
        }

        .interview-setup-card .form-control {
            border-radius: 8px;
            border: 1px solid #dee2e6;
            padding: 12px 15px;
            font-size: 15px;
            transition: border-color 0.3s ease;
        }

        .interview-setup-card .form-control:focus {
            border-color: #FF4357;
            box-shadow: 0 0 0 0.2rem rgba(255, 67, 87, 0.15);
        }

        .interview-setup-card label {
            font-weight: 600;
            color: #495057;
            margin-bottom: 6px;
            font-size: 14px;
        }

        .btn-start-interview {
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: #fff !important;
            border: none;
            border-radius: 8px;
            padding: 14px 40px;
            font-size: 16px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            transition: all 0.3s ease;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.3);
            cursor: pointer;
        }

        .btn-start-interview:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(255, 67, 87, 0.4);
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
            color: #FF4357;
            font-weight: 700;
            font-size: 16px;
            line-height: 1;
        }

        .history-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            padding: 25px 30px;
            border: 1px solid #e9ecef;
            margin-bottom: 15px;
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }

        .history-card:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 24px rgba(0, 0, 0, 0.12);
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

        .score-high { background: #d4edda; color: #155724; }
        .score-mid { background: #fff3cd; color: #856404; }
        .score-low { background: #f8d7da; color: #721c24; }

        .status-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
        }
        .status-pending { background: #fff3cd; color: #856404; }
        .status-completed { background: #d4edda; color: #155724; }
        .status-in-progress { background: #cce5ff; color: #004085; }
        .status-cancelled { background: #f8d7da; color: #721c24; }

        .page-header-section {
            padding: 50px 0 30px;
        }

        .page-header-section h2 {
            font-size: 32px;
            font-weight: 700;
            color: #2d3436;
        }

        .page-header-section p {
            color: #636e72;
            font-size: 16px;
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
                        <h2>AI Interview</h2>
                        <p>Practice your interview skills with our AI-powered interviewer. Get instant feedback and improve your performance.</p>
                    </div>
                    <div class="col-lg-4 text-right pt-3">
                        <a href="InterviewHistory.aspx" class="btn" style="border: 2px solid #FF4357; color: #FFFFFF; border-radius: 8px; padding: 10px 25px; font-weight: 600;">
                            <i class="fas fa-history"></i> View Past Interviews
                        </a>
                    </div>
                </div>
            </div>
        </div>

        <!-- Interview Setup Form -->
        <section class="pt-0" style="padding-bottom: 80px;">
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
                                        <label>Job Role / Position</label>
                                        <asp:TextBox ID="txtRole" runat="server" CssClass="form-control" placeholder="e.g., Software Engineer"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="rfvRole" runat="server" ControlToValidate="txtRole"
                                            ErrorMessage="Role is required" CssClass="text-danger" Display="Dynamic" ValidationGroup="interview"></asp:RequiredFieldValidator>
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label>Experience Level</label>
                                        <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-control">
                                            <asp:ListItem Text="-- Select Level --" Value="" />
                                            <asp:ListItem Text="Junior (0-2 years)" Value="Junior" />
                                            <asp:ListItem Text="Mid-Level (2-5 years)" Value="Mid" />
                                            <asp:ListItem Text="Senior (5+ years)" Value="Senior" />
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="rfvLevel" runat="server" ControlToValidate="ddlLevel"
                                            ErrorMessage="Level is required" CssClass="text-danger" Display="Dynamic" ValidationGroup="interview"></asp:RequiredFieldValidator>
                                    </div>
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <div class="form-group mb-3">
                                        <label>Interview Type</label>
                                        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-control">
                                            <asp:ListItem Text="-- Select Type --" Value="" />
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
                                        <label>Number of Questions</label>
                                        <asp:DropDownList ID="ddlQuestionCount" runat="server" CssClass="form-control">
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

                            <div style="margin-top: 35px; text-align: center;">
                                <asp:Button ID="btnStartInterview" runat="server" Text="Generate Interview" 
                                    CssClass="btn-start-interview" OnClick="btnStartInterview_Click" ValidationGroup="interview" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Recent Interviews Section -->
                <div class="row justify-content-center mt-5">
                    <div class="col-xl-8 col-lg-10">
                        <h4 style="font-weight: 700; color: #2d3436; margin-bottom: 20px;">
                            <i class="fas fa-clock" style="color: #FF4357;"></i> Your Recent Interviews
                        </h4>
                        
                        <asp:Repeater ID="rptRecentInterviews" runat="server">
                            <ItemTemplate>
                                <div class="history-card">
                                    <div class="row align-items-center">
                                        <div class="col-md-5">
                                            <div class="role-title"><%# Eval("Role") %> Interview</div>
                                            <div class="meta-info">
                                                <i class="fas fa-layer-group"></i> <%# Eval("Level") %> &middot;
                                                <i class="fas fa-tag"></i> <%# Eval("InterviewType") %> &middot;
                                                <%# GetTimeAgo(Eval("CreatedAt")) %>
                                            </div>
                                        </div>
                                        <div class="col-md-3 text-center">
                                            <span class='status-badge status-<%# Eval("Status").ToString().ToLower().Replace(" ", "-") %>'>
                                                <%# Eval("Status") %>
                                            </span>
                                        </div>
                                        <div class="col-md-2 text-center">
                                            <%# GetScoreBadge(Eval("TotalScore")) %>
                                        </div>
                                        <div class="col-md-2 text-right">
                                            <a href='<%# GetInterviewLink(Eval("InterviewId"), Eval("Status")) %>' 
                                               class="btn btn-sm" style="border: 1px solid #FF4357; color: #FFFFFF; border-radius: 6px;">
                                                <%# GetActionText(Eval("Status")) %>
                                            </a>
                                            <%# GetRetakeButton(Eval("InterviewId"), Eval("Status")) %>
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
    <div id="loadingOverlay" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(255,255,255,0.85); z-index:9999; justify-content:center; align-items:center; flex-direction:column;">
        <div style="text-align:center;">
            <div style="width:60px;height:60px;border:5px solid #f3f3f3;border-top:5px solid #FF4357;border-radius:50%;animation:spin 1s linear infinite;margin:0 auto 20px;"></div>
            <h4 style="color:#2d3436;font-weight:700;">Generating Your Interview...</h4>
            <p style="color:#636e72;">AI is preparing personalized questions. This may take a few seconds.</p>
            <div id="debugLog" style="margin-top:15px;font-size:12px;color:#888;"></div>
        </div>
    </div>
    <style>@keyframes spin{0%{transform:rotate(0deg)}100%{transform:rotate(360deg)}}</style>

    <script type="text/javascript">
        // Debug logging helper
        function debugLog(msg) {
            console.log('[Interview] ' + msg);
            var logDiv = document.getElementById('debugLog');
            if (logDiv) logDiv.innerHTML += '<div>' + msg + '</div>';
        }

        // Show loading overlay on form submit
        var form = document.querySelector('form');
        if (form) {
            form.addEventListener('submit', function (e) {
                debugLog('Form submit triggered at ' + new Date().toISOString());
                // Check if validation passed
                if (typeof Page_ClientValidate === 'function') {
                    var valid = Page_ClientValidate('interview');
                    debugLog('Client validation result: ' + valid);
                    if (!valid) {
                        debugLog('Validation failed - form will not submit');
                        return;
                    }
                }
                debugLog('Showing loading overlay...');
                var overlay = document.getElementById('loadingOverlay');
                if (overlay) overlay.style.display = 'flex';
                debugLog('PostBack starting - waiting for server response...');
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

        function retakeInterview(interviewId) {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'RetakeInterview.ashx', true);
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        try {
                            var resp = JSON.parse(xhr.responseText);
                            if (resp.success && resp.newInterviewId) {
                                window.location.href = 'TakeInterview.aspx?id=' + resp.newInterviewId;
                                return;
                            }
                        } catch (e) { }
                        alert('Failed to create retake interview.');
                    } else {
                        alert('Failed to retake: ' + xhr.responseText);
                    }
                }
            };
            xhr.send('InterviewId=' + encodeURIComponent(interviewId));
        }
    </script>
</asp:Content>
