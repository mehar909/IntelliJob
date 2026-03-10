<%@ Page Title="Interview Feedback" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="InterviewFeedback.aspx.cs" Inherits="IntelliJob.User.InterviewFeedback" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .feedback-container {
            padding: 50px 0 80px;
        }

        .feedback-header {
            text-align: center;
            margin-bottom: 15px;
        }

        .feedback-header h2 {
            font-size: 28px;
            font-weight: 700;
            color: #2d3436;
        }

        .feedback-header h2 span {
            color: #FF4357;
            text-transform: capitalize;
        }

        .feedback-meta {
            display: flex;
            justify-content: center;
            gap: 30px;
            margin-bottom: 30px;
            flex-wrap: wrap;
        }

        .feedback-meta .meta-item {
            display: flex;
            align-items: center;
            gap: 8px;
            color: #636e72;
            font-size: 15px;
        }

        .feedback-meta .meta-item i {
            color: #FF4357;
        }

        .feedback-meta .meta-item .total-score {
            color: #FF4357;
            font-weight: 700;
            font-size: 18px;
        }

        .feedback-divider {
            border: none;
            border-top: 2px solid #f0f0f0;
            margin: 25px 0;
        }

        .final-assessment {
            background: #f8f9fa;
            border-radius: 12px;
            padding: 25px 30px;
            border-left: 4px solid #FF4357;
            font-size: 15px;
            line-height: 1.7;
            color: #2d3436;
            margin-bottom: 30px;
        }

        /* Score cards */
        .score-breakdown h3 {
            font-size: 20px;
            font-weight: 700;
            color: #2d3436;
            margin-bottom: 20px;
        }

        .score-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 1px solid #e9ecef;
            padding: 25px;
            margin-bottom: 20px;
        }

        .score-card .score-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 12px;
        }

        .score-card .score-header h5 {
            font-size: 16px;
            font-weight: 700;
            color: #2d3436;
            margin: 0;
        }

        .score-card .score-value {
            font-size: 20px;
            font-weight: 700;
        }

        .score-card .score-value.high { color: #00b894; }
        .score-card .score-value.mid { color: #fdcb6e; }
        .score-card .score-value.low { color: #e74c3c; }

        .score-bar {
            background: #f0f0f0;
            border-radius: 10px;
            height: 8px;
            overflow: hidden;
            margin-bottom: 12px;
        }

        .score-bar .fill {
            height: 100%;
            border-radius: 10px;
            transition: width 1s ease;
        }

        .score-bar .fill.high { background: linear-gradient(90deg, #00b894, #55efc4); }
        .score-bar .fill.mid { background: linear-gradient(90deg, #fdcb6e, #ffeaa7); }
        .score-bar .fill.low { background: linear-gradient(90deg, #e74c3c, #fab1a0); }

        .score-card .score-comment {
            color: #636e72;
            font-size: 14px;
            line-height: 1.6;
            margin: 0;
        }

        /* Strengths & Areas */
        .feedback-list-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 1px solid #e9ecef;
            padding: 25px 30px;
            margin-bottom: 20px;
        }

        .feedback-list-card h4 {
            font-size: 18px;
            font-weight: 700;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .feedback-list-card h4 .icon-strengths { color: #00b894; }
        .feedback-list-card h4 .icon-improve { color: #fdcb6e; }

        .feedback-list-card ul {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        .feedback-list-card ul li {
            padding: 8px 0;
            padding-left: 25px;
            position: relative;
            color: #2d3436;
            font-size: 14px;
            border-bottom: 1px solid #f8f9fa;
        }

        .feedback-list-card ul li:last-child { border-bottom: none; }

        .feedback-list-card ul li:before {
            content: "\f00c";
            font-family: "Font Awesome 5 Free";
            font-weight: 900;
            position: absolute;
            left: 0;
            color: #00b894;
        }

        .feedback-list-card.improve ul li:before {
            content: "\f0eb";
            color: #fdcb6e;
        }

        .action-buttons {
            display: flex;
            gap: 15px;
            justify-content: center;
            margin-top: 30px;
            flex-wrap: wrap;
        }

        .action-buttons .btn-back {
            background: #fff;
            border: 2px solid #FF4357;
            color: #FF4357;
            border-radius: 8px;
            padding: 12px 30px;
            font-weight: 600;
            font-size: 15px;
            text-decoration: none;
            transition: all 0.3s ease;
        }

        .action-buttons .btn-back:hover {
            background: #fff5f6;
        }

        .action-buttons .btn-retake {
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: #fff;
            border: none;
            border-radius: 8px;
            padding: 12px 30px;
            font-weight: 600;
            font-size: 15px;
            text-decoration: none;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.3);
            transition: all 0.3s ease;
        }

        .action-buttons .btn-retake:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(255, 67, 87, 0.4);
            color: #fff;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>
        <div class="feedback-container">
            <div class="container">
                <div class="row justify-content-center">
                    <div class="col-xl-8 col-lg-10">

                        <!-- Header -->
                        <div class="feedback-header">
                            <h2>Feedback on the Interview &mdash; <span><asp:Literal ID="litRole" runat="server" /></span></h2>
                        </div>

                        <!-- Meta: Score + Date -->
                        <div class="feedback-meta">
                            <div class="meta-item">
                                <i class="fas fa-star"></i>
                                <span>Overall Impression: <span class="total-score"><asp:Literal ID="litTotalScore" runat="server" /></span>/100</span>
                            </div>
                            <div class="meta-item">
                                <i class="fas fa-calendar-alt"></i>
                                <span><asp:Literal ID="litDate" runat="server" /></span>
                            </div>
                            <div class="meta-item">
                                <i class="fas fa-layer-group"></i>
                                <span><asp:Literal ID="litLevel" runat="server" /></span>
                            </div>
                            <div class="meta-item">
                                <i class="fas fa-tag"></i>
                                <span><asp:Literal ID="litType" runat="server" /></span>
                            </div>
                        </div>

                        <hr class="feedback-divider" />

                        <!-- Final Assessment -->
                        <div class="final-assessment">
                            <asp:Literal ID="litFinalAssessment" runat="server" />
                        </div>

                        <!-- Score Breakdown -->
                        <div class="score-breakdown">
                            <h3>Breakdown of the Interview</h3>

                            <asp:Literal ID="litScoreCards" runat="server" />
                        </div>

                        <!-- Strengths -->
                        <div class="feedback-list-card">
                            <h4><i class="fas fa-check-circle icon-strengths"></i> Strengths</h4>
                            <ul>
                                <asp:Literal ID="litStrengths" runat="server" />
                            </ul>
                        </div>

                        <!-- Areas for Improvement -->
                        <div class="feedback-list-card improve">
                            <h4><i class="fas fa-lightbulb icon-improve"></i> Areas for Improvement</h4>
                            <ul>
                                <asp:Literal ID="litAreas" runat="server" />
                            </ul>
                        </div>

                        <!-- Action Buttons -->
                        <div class="action-buttons">
                            <a href="Interview.aspx" class="btn-back">
                                <i class="fas fa-arrow-left"></i> Back to Dashboard
                            </a>
                            <a href="InterviewHistory.aspx" class="btn-back">
                                <i class="fas fa-history"></i> View History
                            </a>
                            <asp:Panel ID="pnlRetake" runat="server" Visible="false" style="display:inline;">
                                <a href="javascript:void(0)" onclick="retakeInterview()" class="btn-retake" id="btnRetakeLink">
                                    <i class="fas fa-redo"></i> Retake Interview
                                </a>
                            </asp:Panel>
                        </div>

                        <!-- Re-evaluate: always visible for completed interviews with transcript -->
                        <div style="text-align:center; margin-top:20px;">
                            <asp:Button ID="btnReEvaluate" runat="server" Text="Re-evaluate Interview" 
                                OnClick="btnRegenerate_Click" 
                                CssClass="btn-back" style="display:inline-block; cursor:pointer;" 
                                OnClientClick="this.disabled=true;this.value='Re-evaluating... please wait up to 60s';" 
                                UseSubmitBehavior="false" />
                            <p style="color:#b2bec3; font-size:12px; margin-top:6px;">Resubmit the same transcript for a fresh AI evaluation</p>
                        </div>

                        <!-- Regenerate Feedback (shown only when feedback failed) -->
                        <asp:Panel ID="pnlRegenerate" runat="server" Visible="false">
                            <div style="background:#fff3cd; border:1px solid #ffc107; border-radius:12px; padding:20px 25px; margin-top:25px; text-align:center;">
                                <p style="color:#856404; font-size:15px; margin-bottom:15px;">
                                    <i class="fas fa-exclamation-triangle"></i>
                                    Feedback generation failed (likely due to API rate limits). You can retry without retaking the interview.
                                </p>
                                <asp:Button ID="btnRegenerate" runat="server" Text="Regenerate AI Feedback" 
                                    OnClick="btnRegenerate_Click" 
                                    CssClass="btn-retake" style="display:inline-block;" 
                                    OnClientClick="this.disabled=true;this.value='Generating... please wait up to 60s';" 
                                    UseSubmitBehavior="false" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </main>

    <asp:HiddenField ID="hdnInterviewId" runat="server" />

    <script type="text/javascript">
        // Animate score bars on page load
        window.addEventListener('load', function () {
            var fills = document.querySelectorAll('.score-bar .fill');
            for (var i = 0; i < fills.length; i++) {
                var target = fills[i].getAttribute('data-score');
                fills[i].style.width = target + '%';
            }
        });

        function retakeInterview() {
            var btn = document.getElementById('btnRetakeLink');
            btn.style.pointerEvents = 'none';
            btn.style.opacity = '0.6';
            btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Creating...';

            var interviewId = document.getElementById('<%= hdnInterviewId.ClientID %>').value;
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
                    btn.style.pointerEvents = '';
                    btn.style.opacity = '';
                    btn.innerHTML = '<i class="fas fa-redo"></i> Retake Interview';
                }
            };
            xhr.send('InterviewId=' + encodeURIComponent(interviewId));
        }
    </script>
</asp:Content>
