<%@ Page Title="Take Interview" Language="C#" MasterPageFile="~/User/UserMaster.Master" AutoEventWireup="true" CodeBehind="TakeInterview.aspx.cs" Inherits="IntelliJob.User.TakeInterview" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .interview-container {
            min-height: 70vh;
            padding: 40px 0 80px;
        }

        .interview-header-card {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            padding: 25px 30px;
            border: 1px solid #e9ecef;
            margin-bottom: 30px;
        }

        .interview-header-card h3 {
            font-size: 22px;
            font-weight: 700;
            color: #2d3436;
            margin: 0;
        }

        .interview-header-card .interview-meta {
            display: flex;
            gap: 20px;
            margin-top: 8px;
            flex-wrap: wrap;
        }

        .interview-header-card .interview-meta span {
            color: #636e72;
            font-size: 14px;
        }

        .interview-header-card .interview-meta span i {
            color: #FF4357;
            margin-right: 5px;
        }

        /* Two-card layout like the reference repo */
        .call-view {
            display: flex;
            justify-content: center;
            gap: 40px;
            margin: 30px 0;
            flex-wrap: wrap;
        }

        .card-interviewer {
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
            border-radius: 16px;
            padding: 40px;
            text-align: center;
            width: 280px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
            position: relative;
            overflow: hidden;
        }

        .card-interviewer .avatar {
            width: 100px;
            height: 100px;
            border-radius: 50%;
            background: linear-gradient(135deg, #FF4357, #ff6b7a);
            margin: 0 auto 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
            color: #fff;
            position: relative;
        }

        .card-interviewer .avatar .speaking-indicator {
            position: absolute;
            width: 120px;
            height: 120px;
            border-radius: 50%;
            border: 3px solid #FF4357;
            animation: pulse-ring 1.5s ease-out infinite;
            display: none;
        }

        .card-interviewer .avatar .speaking-indicator.active {
            display: block;
        }

        @keyframes pulse-ring {
            0% { transform: scale(0.9); opacity: 1; }
            100% { transform: scale(1.3); opacity: 0; }
        }

        .card-interviewer h4 {
            color: #fff;
            font-size: 18px;
            font-weight: 600;
            margin: 0;
        }

        .card-interviewer p {
            color: #a0a0b0;
            font-size: 13px;
            margin-top: 5px;
        }

        .card-user {
            background: #ffffff;
            border-radius: 16px;
            padding: 40px;
            text-align: center;
            width: 280px;
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
            border: 2px solid #e9ecef;
        }

        .card-user .avatar {
            width: 100px;
            height: 100px;
            border-radius: 50%;
            background: #f0f0f0;
            margin: 0 auto 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
            color: #636e72;
            overflow: hidden;
        }

        .card-user .avatar img {
            width: 100%;
            height: 100%;
            object-fit: cover;
        }

        .card-user h4 {
            color: #2d3436;
            font-size: 18px;
            font-weight: 600;
            margin: 0;
        }

        .card-user p {
            color: #636e72;
            font-size: 13px;
            margin-top: 5px;
        }

        /* Transcript area */
        .transcript-box {
            background: #f8f9fa;
            border-radius: 12px;
            border: 1px solid #e9ecef;
            padding: 20px 25px;
            margin: 20px auto;
            max-width: 700px;
            min-height: 60px;
        }

        .transcript-box p {
            color: #2d3436;
            font-size: 15px;
            margin: 0;
            animation: fadeIn 0.5s ease-in;
        }

        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(5px); }
            to { opacity: 1; transform: translateY(0); }
        }

        .transcript-box .transcript-placeholder {
            color: #b2bec3;
            font-style: italic;
        }

        /* Google Meet-style bottom bar */
        .call-controls {
            text-align: center;
            margin-top: 30px;
        }

        .call-controls-start {
            text-align: center;
        }

        .btn-call {
            background: linear-gradient(135deg, #00b894, #00cec9);
            color: #fff !important;
            border: none;
            border-radius: 50%;
            width: 80px;
            height: 80px;
            font-size: 28px;
            cursor: pointer;
            box-shadow: 0 6px 20px rgba(0, 184, 148, 0.4);
            transition: all 0.3s ease;
        }

        .btn-call:hover {
            transform: scale(1.1);
            box-shadow: 0 8px 28px rgba(0, 184, 148, 0.5);
        }

        .btn-call.connecting {
            animation: pulse-btn 1.5s ease-in-out infinite;
        }

        @keyframes pulse-btn {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.05); }
        }

        .meet-bar {
            display: none;
            position: fixed;
            bottom: 0;
            left: 0;
            right: 0;
            background: #202124;
            padding: 12px 20px;
            z-index: 900;
            justify-content: center;
            align-items: center;
            gap: 16px;
            box-shadow: 0 -2px 20px rgba(0,0,0,0.2);
        }

        .meet-bar .bar-timer {
            position: absolute;
            left: 24px;
            color: #e8eaed;
            font-size: 13px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .meet-bar .bar-timer .total-time {
            font-size: 22px;
            font-weight: 700;
            font-variant-numeric: tabular-nums;
        }

        .meet-bar .bar-timer .total-time.time-warning { color: #fdcb6e; }
        .meet-bar .bar-timer .total-time.time-danger { color: #FF4357; animation: blink-time 1s infinite; }

        @keyframes blink-time {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.5; }
        }

        .meet-bar .bar-timer .q-time {
            font-size: 13px;
            color: #9aa0a6;
            padding: 3px 10px;
            border-radius: 12px;
            background: #303134;
        }

        .meet-bar .bar-timer .q-time.q-warning { color: #fdcb6e; }
        .meet-bar .bar-timer .q-time.q-danger { color: #FF4357; }

        .meet-btn {
            width: 48px;
            height: 48px;
            border-radius: 50%;
            border: none;
            cursor: pointer;
            font-size: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            transition: all 0.2s ease;
            color: #fff;
        }

        .meet-btn:hover { transform: scale(1.08); }
        .meet-btn:disabled { opacity: 0.4; cursor: not-allowed; transform: none; }

        .meet-btn-mic { background: #3c4043; }
        .meet-btn-mic.muted { background: #ea4335; }
        .meet-btn-end { background: #ea4335; width: 56px; border-radius: 28px; }
        .meet-btn-end:hover { background: #d93025; }

        .call-status-text {
            font-size: 14px;
            color: #636e72;
            margin-top: 12px;
        }

        /* Questions panel (for text-based Phase 1) */
        .questions-panel {
            background: #ffffff;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
            border: 1px solid #e9ecef;
            margin-top: 30px;
            overflow: hidden;
        }

        .questions-panel .panel-header {
            background: linear-gradient(135deg, #2d3436, #636e72);
            color: #fff;
            padding: 15px 25px;
            font-size: 16px;
            font-weight: 600;
        }

        .question-item {
            padding: 18px 25px;
            border-bottom: 1px solid #f0f0f0;
            display: flex;
            align-items: flex-start;
            gap: 15px;
        }

        .question-item:last-child {
            border-bottom: none;
        }

        .question-number {
            background: #FF4357;
            color: #fff;
            width: 30px;
            height: 30px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 14px;
            font-weight: 700;
            flex-shrink: 0;
        }

        .question-item.answered .question-number {
            background: #00b894;
        }

        .question-text {
            font-size: 15px;
            color: #2d3436;
            flex: 1;
        }

        .answer-area {
            margin-top: 10px;
        }

        .answer-area textarea {
            width: 100%;
            border: 1px solid #dee2e6;
            border-radius: 8px;
            padding: 10px 15px;
            font-size: 14px;
            resize: vertical;
            min-height: 80px;
        }

        .answer-area textarea:focus {
            border-color: #FF4357;
            outline: none;
            box-shadow: 0 0 0 0.2rem rgba(255, 67, 87, 0.15);
        }

        .btn-submit-answers {
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: #fff !important;
            border: none;
            border-radius: 8px;
            padding: 14px 40px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            margin: 25px auto;
            display: block;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.3);
            transition: all 0.3s ease;
        }

        .btn-submit-answers:hover {
            transform: translateY(-2px);
            box-shadow: 0 6px 16px rgba(255, 67, 87, 0.4);
        }

        /* Mode toggle */
        .mode-toggle {
            display: flex;
            justify-content: center;
            gap: 10px;
            margin: 20px 0;
        }

        .mode-toggle .mode-btn {
            padding: 10px 28px;
            border-radius: 8px;
            font-weight: 600;
            font-size: 14px;
            cursor: pointer;
            border: 2px solid #dee2e6;
            background: #fff;
            color: #636e72;
            transition: all 0.3s ease;
        }

        .mode-toggle .mode-btn.active {
            border-color: #FF4357;
            background: #FF4357;
            color: #fff;
        }

        .mode-toggle .mode-btn:hover:not(.active) {
            border-color: #FF4357;
            color: #FF4357;
        }

        /* Voice transcript messages */
        .voice-messages {
            max-height: 350px;
            overflow-y: auto;
            padding: 0;
        }

        .voice-msg {
            padding: 10px 15px;
            margin: 6px 0;
            border-radius: 10px;
            font-size: 14px;
            line-height: 1.5;
            animation: fadeIn 0.4s ease;
            max-width: 85%;
        }

        .voice-msg.assistant {
            background: #e8f4fd;
            color: #2d3436;
            margin-right: auto;
        }

        .voice-msg.user {
            background: #fff0f1;
            color: #2d3436;
            margin-left: auto;
        }

        .voice-msg .msg-role {
            font-weight: 700;
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            margin-bottom: 3px;
        }

        .voice-msg.assistant .msg-role { color: #0984e3; }
        .voice-msg.user .msg-role { color: #FF4357; }

        .vapi-unavailable {
            background: #fff3cd;
            border: 1px solid #ffc107;
            border-radius: 8px;
            padding: 12px 18px;
            color: #856404;
            font-size: 13px;
            text-align: center;
            margin-top: 10px;
        }

        .voice-msg.partial {
            opacity: 0.6;
            font-style: italic;
        }

        .typing-cursor {
            animation: blink-cursor 0.8s infinite;
            font-weight: 300;
            color: #999;
        }

        @keyframes blink-cursor {
            0%, 100% { opacity: 1; }
            50% { opacity: 0; }
        }

        .voice-msg.redone {
            opacity: 0.5;
            position: relative;
        }

        .voice-msg.redone::after {
            content: 'Redone';
            position: absolute;
            top: 5px;
            right: 10px;
            font-size: 10px;
            color: #e17055;
            font-weight: 700;
            text-transform: uppercase;
        }

        /* Redo link inside transcript */
        .redo-link {
            display: none;
            text-align: right;
            margin: -2px 0 6px;
            max-width: 85%;
            margin-left: auto;
        }

        .redo-link button {
            background: none;
            border: none;
            color: #f39c12;
            font-size: 12px;
            font-weight: 600;
            cursor: pointer;
            padding: 2px 8px;
        }

        .redo-link button:hover { color: #e17055; }
        .redo-link button:disabled { opacity: 0.4; cursor: not-allowed; }

        /* Confirmation Modal */
        .confirm-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0, 0, 0, 0.5);
            z-index: 9998;
            justify-content: center;
            align-items: center;
        }

        .confirm-modal {
            background: #fff;
            border-radius: 16px;
            padding: 30px;
            max-width: 600px;
            width: 90%;
            max-height: 80vh;
            overflow-y: auto;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
        }

        .confirm-modal h4 {
            font-size: 20px;
            font-weight: 700;
            color: #2d3436;
            margin: 0 0 20px;
        }

        .confirm-summary {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 15px;
            margin-bottom: 20px;
            max-height: 400px;
            overflow-y: auto;
        }

        .confirm-item {
            padding: 10px 0;
            border-bottom: 1px solid #e9ecef;
        }

        .confirm-item:last-child { border-bottom: none; }

        .confirm-item .q-label {
            font-weight: 600;
            color: #636e72;
            font-size: 13px;
        }

        .confirm-item .q-text {
            color: #2d3436;
            font-size: 14px;
            margin: 3px 0;
        }

        .confirm-item .a-text {
            color: #0984e3;
            font-size: 14px;
        }

        .confirm-btns {
            display: flex;
            gap: 12px;
            justify-content: flex-end;
            margin-top: 20px;
        }

        .confirm-btns .btn-back {
            padding: 10px 25px;
            border: 2px solid #dee2e6;
            border-radius: 8px;
            background: #fff;
            color: #636e72;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease;
        }

        .confirm-btns .btn-back:hover { border-color: #FF4357; color: #FF4357; }

        .confirm-btns .btn-confirm {
            padding: 10px 25px;
            border: none;
            border-radius: 8px;
            background: linear-gradient(135deg, #FF4357 0%, #ff6b7a 100%);
            color: #fff;
            font-weight: 600;
            cursor: pointer;
            box-shadow: 0 4px 12px rgba(255, 67, 87, 0.3);
            transition: all 0.3s ease;
        }

        .confirm-btns .btn-confirm:hover { transform: translateY(-2px); }

        /* Voice Submit Area */
        .voice-submit-area {
            text-align: center;
            margin: 20px 0;
            display: none;
        }

        /* pad bottom for fixed meet bar */
        .interview-container.bar-active {
            padding-bottom: 100px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <main>
        <div class="interview-container">
            <div class="container">
                <!-- Interview Header -->
                <div class="interview-header-card">
                    <div class="row align-items-center">
                        <div class="col-md-8">
                            <h3><asp:Literal ID="litRole" runat="server" /> Interview</h3>
                            <div class="interview-meta">
                                <span><i class="fas fa-layer-group"></i> <asp:Literal ID="litLevel" runat="server" /></span>
                                <span><i class="fas fa-tag"></i> <asp:Literal ID="litType" runat="server" /></span>
                                <span><i class="fas fa-code"></i> <asp:Literal ID="litTechStack" runat="server" /></span>
                            </div>
                        </div>
                        <div class="col-md-4 text-right">
                            <span class="status-badge" id="statusBadge" style="background: #cce5ff; color: #004085; padding: 6px 16px; border-radius: 12px; font-weight: 600; font-size: 13px;">
                                Ready
                            </span>
                        </div>
                    </div>
                </div>

                <!-- AI Interviewer + User Cards -->
                <div class="call-view">
                    <div class="card-interviewer">
                        <div class="avatar">
                            <i class="fas fa-robot"></i>
                            <span class="speaking-indicator" id="speakingIndicator"></span>
                        </div>
                        <h4>AI Interviewer</h4>
                        <p>Powered by AI</p>
                    </div>

                    <div class="card-user">
                        <div class="avatar">
                            <asp:Literal ID="litUserAvatar" runat="server" />
                        </div>
                        <h4><asp:Literal ID="litUserName" runat="server" /></h4>
                        <p>Candidate</p>
                    </div>
                </div>

                <!-- Mode Toggle -->
                <div class="mode-toggle">
                    <button type="button" class="mode-btn active" id="btnTextMode" onclick="switchMode('text')">
                        <i class="fas fa-keyboard"></i> Text Mode
                    </button>
                    <button type="button" class="mode-btn" id="btnVoiceMode" onclick="switchMode('voice')">
                        <i class="fas fa-microphone"></i> Voice Mode
                    </button>
                </div>

                <!-- Voice Interview Section (hidden by default) -->
                <div id="voiceSection" style="display:none;">
                    <!-- Live Transcript -->
                    <div class="transcript-box" id="transcriptBox">
                        <div class="voice-messages" id="voiceMessages">
                            <p class="transcript-placeholder" id="transcriptPlaceholder">Click the green call button to start your voice interview.</p>
                        </div>
                        <!-- Redo link appears after user messages inside the transcript -->
                        <div class="redo-link" id="redoLink">
                            <button type="button" onclick="redoLastMessage()" id="btnRedoInline"><i class="fas fa-undo"></i> Redo last answer</button>
                        </div>
                    </div>

                    <!-- Start Call (shown before call) -->
                    <div class="call-controls call-controls-start" id="callControlsStart">
                        <button type="button" class="btn-call" id="btnStartCall" onclick="startVoiceCall()">
                            <i class="fas fa-phone"></i>
                        </button>
                        <div class="call-status-text" id="callStatusText">Click to start voice interview</div>
                    </div>
                </div>

                <!-- Voice Submit Area (shown after call ends) -->
                <div class="voice-submit-area" id="voiceSubmitArea">
                    <p style="color: #636e72; margin-bottom: 15px;"><i class="fas fa-check-circle" style="color: #00b894;"></i> Voice interview complete. Review the transcript above, then submit for evaluation.</p>
                    <div style="display:flex; gap:12px; justify-content:center; flex-wrap:wrap;">
                        <button type="button" class="btn-submit-answers" onclick="confirmVoiceSubmit()">
                            <i class="fas fa-paper-plane"></i> Submit for Evaluation
                        </button>
                        <button type="button" onclick="cancelInterview()" style="background:#fff; border:2px solid #e74c3c; color:#e74c3c; border-radius:8px; padding:14px 30px; font-size:16px; font-weight:600; cursor:pointer; transition:all 0.3s ease;">
                            <i class="fas fa-times"></i> Cancel Interview
                        </button>
                    </div>
                </div>

                <!-- Text Interview Section (visible by default) -->
                <div class="questions-panel">
                    <div class="panel-header">
                        <i class="fas fa-question-circle"></i> Interview Questions
                        <span style="float: right; font-weight: 400; font-size: 14px;">
                            Answer each question thoughtfully
                        </span>
                    </div>

                    <asp:Repeater ID="rptQuestions" runat="server">
                        <ItemTemplate>
                            <div class="question-item" id='q_<%# Eval("QuestionId") %>'>
                                <div class="question-number"><%# Eval("SortOrder") %></div>
                                <div style="flex: 1;">
                                    <div class="question-text"><%# Eval("QuestionText") %></div>
                                    <div class="answer-area">
                                        <textarea name='answer_<%# Eval("QuestionId") %>' 
                                            placeholder="Type your answer here..." 
                                            data-questionid='<%# Eval("QuestionId") %>'></textarea>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:HiddenField ID="hdnInterviewId" runat="server" />
                <asp:HiddenField ID="hdnAnswersJson" runat="server" />
                <asp:HiddenField ID="hdnQuestionsJson" runat="server" />

                <div id="textSubmitArea" style="text-align: center; margin-top: 25px; display:flex; gap:12px; justify-content:center; flex-wrap:wrap;">
                    <asp:Button ID="btnSubmitAnswers" runat="server" Text="Submit Answers & Get Feedback" 
                        CssClass="btn-submit-answers" OnClick="btnSubmitAnswers_Click" 
                        OnClientClick="return collectAnswers();" />
                    <button type="button" onclick="cancelInterview()" style="background:#fff; border:2px solid #e74c3c; color:#e74c3c; border-radius:8px; padding:14px 30px; font-size:16px; font-weight:600; cursor:pointer; transition:all 0.3s ease;">
                        <i class="fas fa-times"></i> Cancel Interview
                    </button>
                </div>
            </div>
        </div>

        <!-- Confirmation Modal -->
        <div class="confirm-overlay" id="confirmOverlay">
            <div class="confirm-modal">
                <h4><i class="fas fa-clipboard-check" style="color: #FF4357;"></i> Review Your Answers</h4>
                <p style="color: #636e72; font-size: 14px; margin-bottom: 15px;">Please review your answers before submitting for AI evaluation.</p>
                <div class="confirm-summary" id="confirmSummary"></div>
                <div class="confirm-btns">
                    <button type="button" class="btn-back" onclick="closeConfirmation()">
                        <i class="fas fa-arrow-left"></i> Go Back
                    </button>
                    <button type="button" class="btn-confirm" id="btnFinalSubmit" onclick="finalSubmit()">
                        <i class="fas fa-check"></i> Confirm & Submit
                    </button>
                </div>
            </div>
        </div>

        <!-- Google Meet-style Bottom Bar -->
        <div class="meet-bar" id="meetBar">
            <div class="bar-timer">
                <span class="total-time" id="totalTimeCounter">00:00</span>
                <span class="q-time" id="qTimeCounter" style="display:none;"><i class="fas fa-comment-dots"></i> <span id="qTimeText">--</span></span>
            </div>
            <button type="button" class="meet-btn meet-btn-mic" id="btnMicToggle" onclick="toggleMic()" title="Toggle microphone">
                <i class="fas fa-microphone" id="micIcon"></i>
            </button>
            <button type="button" class="meet-btn meet-btn-end" onclick="endVoiceCall()" title="End call">
                <i class="fas fa-phone-slash"></i>
            </button>
            <button type="button" class="meet-btn" onclick="cancelDuringCall()" title="Cancel interview" style="background:#3c4043; font-size:14px;">
                <i class="fas fa-times-circle"></i>
            </button>
        </div>
    </main>

    <!-- Hide Vapi widget button (we use our own UI) -->
    <style>#vapi-support-btn { display: none !important; }</style>

    <!-- Loading Overlay -->
    <div id="feedbackOverlay" style="display:none; position:fixed; top:0; left:0; width:100%; height:100%; background:rgba(255,255,255,0.9); z-index:9999; justify-content:center; align-items:center; flex-direction:column;">
        <div style="text-align:center;">
            <div style="width:60px;height:60px;border:5px solid #f3f3f3;border-top:5px solid #FF4357;border-radius:50%;animation:fbspin 1s linear infinite;margin:0 auto 20px;"></div>
            <h4 style="color:#2d3436;font-weight:700;" id="overlayTitle">Analyzing Your Interview...</h4>
            <p style="color:#636e72;" id="overlayText">AI is evaluating your answers. This may take 10-15 seconds.</p>

        </div>
    </div>
    <style>@keyframes fbspin{0%{transform:rotate(0deg)}100%{transform:rotate(360deg)}}</style>

    <script type="text/javascript">
        // === Config ===
        var VAPI_TOKEN = '<%= System.Configuration.ConfigurationManager.AppSettings["VapiWebToken"] %>';
        var INTERVIEW_ID = '<%= hdnInterviewId.ClientID %>';
        var interviewQuestions = [];
        try {
            var qJson = document.getElementById('<%= hdnQuestionsJson.ClientID %>').value;
            if (qJson) interviewQuestions = JSON.parse(qJson);
        } catch (e) { console.warn('Could not parse questions JSON', e); }

        // === Debug Logging (console only) ===
        function feedbackLog(msg) {
            console.log('[TakeInterview] ' + msg);
        }

        // === Mode Switching ===
        function switchMode(mode) {
            var voiceSection = document.getElementById('voiceSection');
            var textSection = document.querySelector('.questions-panel');
            var textSubmit = document.getElementById('textSubmitArea');
            var btnText = document.getElementById('btnTextMode');
            var btnVoice = document.getElementById('btnVoiceMode');

            if (mode === 'voice') {
                if (!VAPI_TOKEN || VAPI_TOKEN === 'YOUR_VAPI_WEB_TOKEN_HERE') {
                    alert('Voice mode requires a Vapi Web Token. Add it to Web.config under VapiWebToken.');
                    return;
                }
                voiceSection.style.display = 'block';
                textSection.style.display = 'none';
                textSubmit.style.display = 'none';
                btnVoice.classList.add('active');
                btnText.classList.remove('active');
                feedbackLog('Switched to VOICE mode');
            } else {
                voiceSection.style.display = 'none';
                textSection.style.display = 'block';
                textSubmit.style.display = 'block';
                btnText.classList.add('active');
                btnVoice.classList.remove('active');
                feedbackLog('Switched to TEXT mode');
            }
        }

        // === Text Mode: Collect answers ===
        function collectAnswers() {
            feedbackLog('Collecting answers from textareas...');
            var answers = {};
            var textareas = document.querySelectorAll('.answer-area textarea');
            var hasAnswer = false;
            var answeredCount = 0;

            for (var i = 0; i < textareas.length; i++) {
                var qid = textareas[i].getAttribute('data-questionid');
                var val = textareas[i].value.trim();
                if (val) {
                    answers[qid] = val;
                    hasAnswer = true;
                    answeredCount++;
                }
            }

            if (!hasAnswer) {
                alert('Please answer at least one question before submitting.');
                return false;
            }

            // Store answers and show confirmation modal
            window._pendingAnswers = answers;
            showTextConfirmation(answers);
            return false;
        }

        // === Voice Mode: Vapi Integration ===
        var vapiInstance = null;
        var voiceTranscript = [];
        var callActive = false;
        var micMuted = false;

        // === Timers ===
        var totalInterviewTime = 0;
        var totalTimeLeft = 0;
        var totalTimerId = null;

        var perQuestionTime = 120;
        var questionTimeLeft = 0;
        var questionTimerId = null;
        var questionTimerPaused = false; // paused while user speaks

        // Calculate per-question response time based on experience level
        var INTERVIEW_LEVEL = '';
        try {
            var metaSpans = document.querySelectorAll('.interview-meta span');
            if (metaSpans.length > 0) INTERVIEW_LEVEL = metaSpans[0].textContent.trim().toLowerCase();
        } catch (e) { }

        function getPerQuestionTime(level) {
            if (level.indexOf('junior') >= 0 || level.indexOf('entry') >= 0 || level.indexOf('fresher') >= 0) return 90;
            if (level.indexOf('senior') >= 0 || level.indexOf('expert') >= 0 || level.indexOf('lead') >= 0) return 180;
            return 120;
        }
        perQuestionTime = getPerQuestionTime(INTERVIEW_LEVEL);
        totalInterviewTime = perQuestionTime * interviewQuestions.length;
        totalTimeLeft = totalInterviewTime;

        function formatTime(secs) {
            var m = Math.floor(secs / 60);
            var s = secs % 60;
            return (m < 10 ? '0' : '') + m + ':' + (s < 10 ? '0' : '') + s;
        }

        function startTotalTimer() {
            totalTimeLeft = totalInterviewTime;
            updateTotalTimerDisplay();
            totalTimerId = setInterval(function () {
                totalTimeLeft--;
                updateTotalTimerDisplay();
                if (totalTimeLeft <= 0) {
                    totalTimeLeft = 0;
                    clearInterval(totalTimerId);
                    totalTimerId = null;
                    feedbackLog('Total interview time expired — ending call');
                    endVoiceCall();
                }
            }, 1000);
        }

        function stopTotalTimer() {
            if (totalTimerId) { clearInterval(totalTimerId); totalTimerId = null; }
        }

        function updateTotalTimerDisplay() {
            var el = document.getElementById('totalTimeCounter');
            el.textContent = formatTime(totalTimeLeft);
            el.classList.remove('time-warning', 'time-danger');
            if (totalTimeLeft <= 30) el.classList.add('time-danger');
            else if (totalTimeLeft <= 60) el.classList.add('time-warning');
        }

        function startQuestionTimer() {
            stopQuestionTimer();
            questionTimeLeft = perQuestionTime;
            questionTimerPaused = false;
            var qEl = document.getElementById('qTimeCounter');
            qEl.style.display = 'inline-block';
            updateQuestionTimerDisplay();

            questionTimerId = setInterval(function () {
                if (questionTimerPaused) return;
                questionTimeLeft--;
                updateQuestionTimerDisplay();
                if (questionTimeLeft <= 0) {
                    questionTimeLeft = 0;
                    stopQuestionTimer();
                }
            }, 1000);
        }

        function stopQuestionTimer() {
            if (questionTimerId) { clearInterval(questionTimerId); questionTimerId = null; }
        }

        function pauseQuestionTimer() {
            questionTimerPaused = true;
        }

        function updateQuestionTimerDisplay() {
            var el = document.getElementById('qTimeText');
            el.textContent = formatTime(questionTimeLeft);
            var container = document.getElementById('qTimeCounter');
            container.classList.remove('q-warning', 'q-danger');
            if (questionTimeLeft <= Math.floor(perQuestionTime * 0.25)) container.classList.add('q-danger');
            else if (questionTimeLeft <= Math.floor(perQuestionTime * 0.5)) container.classList.add('q-warning');
        }

        // === Load Vapi SDK (official browser bundle) ===
        (function (d, t) {
            var g = document.createElement(t),
                s = d.getElementsByTagName(t)[0];
            g.src = 'https://cdn.jsdelivr.net/gh/VapiAI/html-script-tag@latest/dist/assets/index.js';
            g.defer = true;
            g.async = true;
            s.parentNode.insertBefore(g, s);
            g.onload = function () {
                feedbackLog('Vapi SDK script loaded');
                if (!VAPI_TOKEN || VAPI_TOKEN === 'YOUR_VAPI_WEB_TOKEN_HERE') return;
                try {
                    vapiInstance = window.vapiSDK.run({
                        apiKey: VAPI_TOKEN,
                        assistant: 'placeholder',
                        config: {
                            position: 'bottom-right',
                            offset: '-9999px',
                            width: '1px',
                            height: '1px',
                            idle: { color: 'transparent', type: 'round' },
                            loading: { color: 'transparent', type: 'round' },
                            active: { color: 'transparent', type: 'round' }
                        }
                    });
                    if (vapiInstance) {
                        setupVapiEvents();
                        feedbackLog('Vapi SDK initialized successfully');
                    }
                } catch (e) {
                    feedbackLog('Error initializing Vapi: ' + e.message);
                }
            };
        })(document, 'script');

        function startVoiceCall() {
            if (callActive) return;

            if (!VAPI_TOKEN || VAPI_TOKEN === 'YOUR_VAPI_WEB_TOKEN_HERE') {
                alert('Voice mode requires a Vapi Web Token. Configure it in Web.config.');
                return;
            }

            voiceTranscript = [];
            micMuted = false;

            if (!vapiInstance) {
                alert('Vapi SDK is still loading. Please try again in a moment.');
                return;
            }

            feedbackLog('Starting voice call...');

            try {

                // Build questions list for the assistant prompt
                var formattedQuestions = '';
                if (interviewQuestions && interviewQuestions.length > 0) {
                    formattedQuestions = interviewQuestions.map(function (q) { return '- ' + q; }).join('\n');
                }

                var totalMin = Math.floor(totalInterviewTime / 60);

                // Update UI to connecting state
                document.getElementById('btnStartCall').classList.add('connecting');
                document.getElementById('callStatusText').textContent = 'Connecting...';
                document.getElementById('transcriptPlaceholder').textContent = 'Connecting to AI interviewer...';

                // Start call with inline assistant configuration
                vapiInstance.start({
                    name: "AI Interviewer",
                    firstMessage: "Hello! Thank you for taking the time to speak with me today. We have about " + totalMin + " minutes for this interview. Let's get started.",
                    silenceTimeoutSeconds: 30,
                    transcriber: {
                        provider: "deepgram",
                        model: "nova-2",
                        language: "en"
                    },
                    voice: {
                        provider: "vapi",
                        voiceId: "Elliot"
                    },
                    model: {
                        provider: "google",
                        model: "gemini-2.0-flash",
                        messages: [
                            {
                                role: "system",
                                content: "You are a professional job interviewer conducting a real-time voice interview with a candidate. Your task is to ask the candidate the following interview questions one by one. Follow these guidelines:\n\n" +
                                    "Interview Questions:\n" + formattedQuestions + "\n\n" +
                                    "Total interview time: " + totalMin + " minutes.\n\n" +
                                    "Guidelines:\n" +
                                    "- Ask ONE question at a time and wait for the candidate to respond\n" +
                                    "- Be PATIENT with silence. The candidate may need time to think. Do NOT interrupt or move on if there is a brief pause. Wait at least 15-20 seconds of complete silence before gently prompting.\n" +
                                    "- Do NOT say filler acknowledgments like 'okay', 'alright', 'great' while the candidate is still forming their answer. Only acknowledge AFTER they have clearly finished speaking.\n" +
                                    "- Listen carefully and provide natural, brief acknowledgments only after the candidate completes their answer\n" +
                                    "- You may ask brief follow-up questions for clarity\n" +
                                    "- Keep a professional and encouraging tone\n" +
                                    "- After all questions are asked, if there is time remaining, ask the candidate if they have any questions or if they would like to add anything, then thank them and end the interview\n" +
                                    "- Do NOT evaluate or score the candidate during the interview\n" +
                                    "- Speak naturally as if in a real interview conversation\n" +
                                    "- Be concise in your responses, do not give long lectures"
                            }
                        ]
                    }
                });

                feedbackLog('Vapi call starting...');

            } catch (err) {
                feedbackLog('ERROR starting voice call: ' + err.message);
                alert('Failed to start voice call: ' + err.message);
                resetCallUI();
            }
        }

        function setupVapiEvents() {
            vapiInstance.on('call-start', function () {
                feedbackLog('Voice call started');
                callActive = true;

                // Hide start controls, show meet bar
                document.getElementById('callControlsStart').style.display = 'none';
                document.getElementById('meetBar').style.display = 'flex';
                document.querySelector('.interview-container').classList.add('bar-active');
                document.getElementById('callStatusText').textContent = '';

                document.getElementById('statusBadge').textContent = 'In Progress';
                document.getElementById('statusBadge').style.background = '#d4edda';
                document.getElementById('statusBadge').style.color = '#155724';
                document.getElementById('transcriptPlaceholder').style.display = 'none';
                document.getElementById('voiceSubmitArea').style.display = 'none';

                // Start total timer
                startTotalTimer();
            });

            vapiInstance.on('call-end', function () {
                feedbackLog('Voice call ended. Transcript messages: ' + voiceTranscript.length);
                callActive = false;
                stopTotalTimer();
                stopQuestionTimer();

                // Hide meet bar, show start controls
                document.getElementById('meetBar').style.display = 'none';
                document.querySelector('.interview-container').classList.remove('bar-active');
                document.getElementById('callControlsStart').style.display = 'block';
                document.getElementById('redoLink').style.display = 'none';

                resetCallUI();

                if (voiceTranscript.length > 0) {
                    document.getElementById('callStatusText').textContent = 'Call ended. Review your transcript and submit.';
                    document.getElementById('voiceSubmitArea').style.display = 'block';
                } else {
                    feedbackLog('WARNING: No transcript messages captured');
                    document.getElementById('callStatusText').textContent = 'No transcript captured. Try again or use text mode.';
                }
            });

            vapiInstance.on('message', function (message) {
                if (message.type === 'transcript') {
                    var role = message.role || 'user';
                    var content = message.transcript || '';
                    if (!content.trim()) return;

                    if (message.transcriptType === 'partial') {
                        // Show live words as they are spoken
                        showPartialTranscript(role, content);
                        return;
                    }

                    if (message.transcriptType === 'final') {
                        // Remove partial bubble, show final
                        removePartialTranscript(role);

                        // When user speaks, pause the question timer (don't stop/reset)
                        if (role === 'user') {
                            pauseQuestionTimer();
                        }

                        feedbackLog('[' + role + '] ' + content.substring(0, 80) + (content.length > 80 ? '...' : ''));

                        // Merge consecutive messages of same role in transcript array
                        if (voiceTranscript.length > 0 && voiceTranscript[voiceTranscript.length - 1].role === role) {
                            var sep = role === 'assistant' ? ' ' : '\n';
                            voiceTranscript[voiceTranscript.length - 1].content += sep + content;
                        } else {
                            voiceTranscript.push({ role: role, content: content });
                        }

                        appendTranscriptMessage(role, content);

                        // Show/hide redo link
                        if (role === 'user') {
                            document.getElementById('redoLink').style.display = 'block';
                            document.getElementById('btnRedoInline').disabled = false;
                        } else {
                            document.getElementById('redoLink').style.display = 'none';
                        }
                    }
                }
            });

            vapiInstance.on('speech-start', function () {
                document.getElementById('speakingIndicator').classList.add('active');
                // AI is speaking → start a new question timer
                startQuestionTimer();
            });

            vapiInstance.on('speech-end', function () {
                document.getElementById('speakingIndicator').classList.remove('active');
                // AI finished speaking → timer keeps running for user to respond
            });

            vapiInstance.on('error', function (err) {
                feedbackLog('Vapi error: ' + JSON.stringify(err));
                console.error('Vapi error:', err);
            });
        }

        function appendTranscriptMessage(role, content) {
            var container = document.getElementById('voiceMessages');
            var lastChild = container.lastElementChild;

            // Merge consecutive messages of same role into one bubble
            if (lastChild && lastChild.classList.contains('voice-msg') && lastChild.classList.contains(role)) {
                var contentDiv = lastChild.querySelector('.msg-content');
                if (contentDiv) {
                    var sep = role === 'assistant' ? ' ' : '<br>';
                    contentDiv.innerHTML += sep + escapeHtml(content);
                    container.scrollTop = container.scrollHeight;
                    return;
                }
            }

            var div = document.createElement('div');
            div.className = 'voice-msg ' + role;
            div.innerHTML = '<div class="msg-role">' + (role === 'assistant' ? 'AI Interviewer' : 'You') + '</div>' +
                '<div class="msg-content">' + escapeHtml(content) + '</div>';
            container.appendChild(div);
            container.scrollTop = container.scrollHeight;
        }

        function showPartialTranscript(role, content) {
            var container = document.getElementById('voiceMessages');
            var partialId = 'partial-' + role;
            var existing = document.getElementById(partialId);

            if (existing) {
                existing.querySelector('.msg-content').innerHTML = escapeHtml(content) + '<span class="typing-cursor">|</span>';
            } else {
                var div = document.createElement('div');
                div.className = 'voice-msg ' + role + ' partial';
                div.id = partialId;
                div.innerHTML = '<div class="msg-role">' + (role === 'assistant' ? 'AI Interviewer' : 'You') + '</div>' +
                    '<div class="msg-content">' + escapeHtml(content) + '<span class="typing-cursor">|</span></div>';
                container.appendChild(div);
            }
            container.scrollTop = container.scrollHeight;
        }

        function removePartialTranscript(role) {
            var el = document.getElementById('partial-' + role);
            if (el) el.remove();
        }

        function escapeHtml(text) {
            var div = document.createElement('div');
            div.appendChild(document.createTextNode(text));
            return div.innerHTML;
        }

        function endVoiceCall() {
            if (vapiInstance && callActive) {
                feedbackLog('Ending voice call...');
                vapiInstance.stop();
            }
        }

        function resetCallUI() {
            document.getElementById('btnStartCall').style.display = 'inline-block';
            document.getElementById('btnStartCall').classList.remove('connecting');
        }

        // === Mic Toggle ===
        function toggleMic() {
            if (!vapiInstance || !callActive) return;
            micMuted = !micMuted;
            vapiInstance.setMuted(micMuted);
            var btn = document.getElementById('btnMicToggle');
            var icon = document.getElementById('micIcon');
            if (micMuted) {
                btn.classList.add('muted');
                icon.className = 'fas fa-microphone-slash';
            } else {
                btn.classList.remove('muted');
                icon.className = 'fas fa-microphone';
            }
            feedbackLog('Mic ' + (micMuted ? 'muted' : 'unmuted'));
        }

        // === Redo Last Message ===
        function redoLastMessage() {
            if (!callActive) return;

            var container = document.getElementById('voiceMessages');
            var userMsgs = container.querySelectorAll('.voice-msg.user');
            if (userMsgs.length === 0) return;

            var lastUserMsg = userMsgs[userMsgs.length - 1];
            if (lastUserMsg.classList.contains('redone')) return;

            lastUserMsg.classList.add('redone');

            // Mark in transcript array (no timer reset — same time frame)
            for (var i = voiceTranscript.length - 1; i >= 0; i--) {
                if (voiceTranscript[i].role === 'user' && voiceTranscript[i].content.indexOf('[REDONE]') !== 0) {
                    voiceTranscript[i].content = '[REDONE] ' + voiceTranscript[i].content;
                    break;
                }
            }

            feedbackLog('Last user message marked as redone');

            // Hide redo after use
            document.getElementById('redoLink').style.display = 'none';
        }

        function saveVoiceTranscript() {
            feedbackLog('Saving transcript to server (' + voiceTranscript.length + ' messages)...');

            // Show loading overlay
            var overlay = document.getElementById('feedbackOverlay');
            document.getElementById('overlayTitle').textContent = 'Processing Voice Interview...';
            document.getElementById('overlayText').textContent = 'Saving transcript and generating AI feedback. This may take 15-30 seconds.';
            if (overlay) overlay.style.display = 'flex';

            var interviewId = document.getElementById('<%= hdnInterviewId.ClientID %>').value;

            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'SaveVoiceTranscript.ashx', true);
            xhr.setRequestHeader('Content-Type', 'application/json');
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        feedbackLog('Transcript saved. Redirecting to feedback...');
                        window.location.href = 'InterviewFeedback.aspx?id=' + interviewId;
                    } else {
                        feedbackLog('ERROR saving transcript: ' + xhr.responseText);
                        overlay.style.display = 'none';
                        alert('Failed to save transcript. Error: ' + xhr.responseText + '\nYou can still use text mode.');
                    }
                }
            };
            xhr.send(JSON.stringify({
                InterviewId: parseInt(interviewId),
                Messages: voiceTranscript
            }));
        }

        // === Confirmation Modal Functions ===
        function showTextConfirmation(answers) {
            var summary = document.getElementById('confirmSummary');
            var html = '';
            var questions = document.querySelectorAll('.question-item');

            for (var i = 0; i < questions.length; i++) {
                var qText = questions[i].querySelector('.question-text').textContent;
                var textarea = questions[i].querySelector('textarea');
                var qid = textarea.getAttribute('data-questionid');
                var answer = answers[qid] || '';

                html += '<div class="confirm-item">';
                html += '<div class="q-label">Question ' + (i + 1) + '</div>';
                html += '<div class="q-text">' + escapeHtml(qText) + '</div>';
                if (answer) {
                    html += '<div class="a-text"><i class="fas fa-check" style="color:#00b894;margin-right:5px;"></i>' + escapeHtml(answer) + '</div>';
                } else {
                    html += '<div class="a-text" style="color:#b2bec3;"><i class="fas fa-minus" style="margin-right:5px;"></i>Not answered</div>';
                }
                html += '</div>';
            }

            summary.innerHTML = html;
            document.getElementById('btnFinalSubmit').setAttribute('onclick', 'finalSubmit()');
            document.getElementById('confirmOverlay').style.display = 'flex';
        }

        function closeConfirmation() {
            document.getElementById('confirmOverlay').style.display = 'none';
        }

        function finalSubmit() {
            if (window._pendingAnswers) {
                var jsonStr = JSON.stringify(window._pendingAnswers);
                document.getElementById('<%= hdnAnswersJson.ClientID %>').value = jsonStr;
                feedbackLog('Submitting to server...');

                var overlay = document.getElementById('feedbackOverlay');
                if (overlay) overlay.style.display = 'flex';
                document.getElementById('confirmOverlay').style.display = 'none';

                __doPostBack('<%= btnSubmitAnswers.UniqueID %>', '');
            }
        }

        function confirmVoiceSubmit() {
            var summary = document.getElementById('confirmSummary');
            var msgCount = voiceTranscript.length;
            var userMsgCount = voiceTranscript.filter(function(m) { return m.role === 'user'; }).length;

            summary.innerHTML = '<div style="text-align:center; padding:20px;">' +
                '<i class="fas fa-microphone" style="font-size:40px; color:#FF4357; margin-bottom:15px;"></i>' +
                '<p style="font-size:16px; color:#2d3436; margin-bottom:8px;">Voice interview recorded successfully.</p>' +
                '<p style="color:#636e72; font-size:14px;">' + msgCount + ' transcript messages captured (' + userMsgCount + ' from you).</p>' +
                '<p style="color:#636e72; font-size:13px;">Submit to receive AI-generated feedback on your performance.</p>' +
                '</div>';

            document.getElementById('btnFinalSubmit').setAttribute('onclick', 'finalVoiceSubmit()');
            document.getElementById('confirmOverlay').style.display = 'flex';
        }

        function finalVoiceSubmit() {
            document.getElementById('confirmOverlay').style.display = 'none';
            saveVoiceTranscript();
        }

        function cancelInterview() {
            if (!confirm('Are you sure you want to cancel this interview? You can retake it later with the same questions.')) return;

            var interviewId = document.getElementById('<%= hdnInterviewId.ClientID %>').value;
            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'CancelInterview.ashx', true);
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
            xhr.onreadystatechange = function () {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        window.location.href = 'Interview.aspx';
                    } else {
                        alert('Failed to cancel interview: ' + xhr.responseText);
                    }
                }
            };
            xhr.send('InterviewId=' + encodeURIComponent(interviewId));
        }

        function cancelDuringCall() {
            if (!confirm('Cancel this interview? The call will end and the interview will be saved as cancelled. You can retake it later.')) return;
            if (vapiInstance && callActive) {
                vapiInstance.stop();
            }
            // Small delay to let call-end fire, then cancel
            setTimeout(function () {
                var interviewId = document.getElementById('<%= hdnInterviewId.ClientID %>').value;
                var xhr = new XMLHttpRequest();
                xhr.open('POST', 'CancelInterview.ashx', true);
                xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                xhr.onreadystatechange = function () {
                    if (xhr.readyState === 4) {
                        window.location.href = 'Interview.aspx';
                    }
                };
                xhr.send('InterviewId=' + encodeURIComponent(interviewId));
            }, 500);
        }
    </script>
</asp:Content>
