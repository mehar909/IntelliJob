<%@ Page Title="Candidate Interview Report" Language="C#"
    MasterPageFile="~/Company/CompanyMaster.Master"
    AutoEventWireup="true" CodeBehind="ViewCandidateReport.aspx.cs"
    Inherits="IntelliJob.Company.ViewCandidateReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<link href="https://fonts.googleapis.com/css2?family=Sora:wght@400;600;700;800&family=DM+Sans:ital,wght@0,400;0,500;1,400&display=swap" rel="stylesheet">
<style>
    :root {
        --brand:       #5b21b6;
        --brand-mid:   #7c3aed;
        --brand-light: #ede9fe;
        --ink:         #1e1b2e;
        --muted:       #6b7280;
        --border:      #e5e7eb;
        --green:       #059669;
        --green-bg:    #d1fae5;
        --amber:       #d97706;
        --amber-bg:    #fef3c7;
        --red:         #dc2626;
        --red-bg:      #fee2e2;
        --radius:      14px;
        --shadow:      0 4px 24px rgba(91,33,182,.10);
    }
    body, .container-fluid { font-family: 'DM Sans', sans-serif; background: #f0eef8; }

    .btn-back {
        display: inline-flex; align-items: center; gap: 8px;
        background: #fff; border: 1.5px solid var(--border);
        color: var(--ink); padding: 8px 20px; border-radius: 50px;
        font-size: 13px; font-weight: 600; text-decoration: none;
        transition: all .2s; box-shadow: 0 1px 4px rgba(0,0,0,.06);
        margin-bottom: 22px;
    }
    .btn-back:hover { background: var(--brand-light); border-color: var(--brand-mid); color: var(--brand); text-decoration: none; }

    /* Hero */
    .report-hero {
        background: linear-gradient(130deg, #2e1065 0%, #5b21b6 50%, #7c3aed 100%);
        border-radius: var(--radius); padding: 36px 40px; margin-bottom: 28px;
        position: relative; overflow: hidden;
    }
    .report-hero::before {
        content: ''; position: absolute; top: -80px; right: -80px;
        width: 300px; height: 300px; border-radius: 50%;
        background: rgba(255,255,255,.05); pointer-events: none;
    }
    .report-hero::after {
        content: ''; position: absolute; bottom: -90px; right: 140px;
        width: 220px; height: 220px; border-radius: 50%;
        background: rgba(255,255,255,.04); pointer-events: none;
    }
    .hero-layout {
        display: flex; justify-content: space-between; align-items: center;
        gap: 24px; flex-wrap: wrap; position: relative; z-index: 1;
    }
    .candidate-avatar {
        width: 64px; height: 64px; border-radius: 50%;
        background: rgba(255,255,255,.2); border: 2px solid rgba(255,255,255,.35);
        display: flex; align-items: center; justify-content: center;
        font-family: 'Sora', sans-serif; font-size: 26px; font-weight: 800;
        color: #fff; flex-shrink: 0;
    }
    .candidate-name {
        font-family: 'Sora', sans-serif; font-size: 22px; font-weight: 700;
        color: #fff; margin: 0 0 3px;
    }
    .candidate-email { color: rgba(255,255,255,.72); font-size: 13.5px; margin: 0 0 12px; }
    .meta-pills { display: flex; flex-wrap: wrap; gap: 6px; }
    .meta-pill {
        background: rgba(255,255,255,.14); backdrop-filter: blur(6px);
        border: 1px solid rgba(255,255,255,.18); color: #fff;
        border-radius: 50px; padding: 4px 13px; font-size: 12.5px; font-weight: 500;
        display: inline-flex; align-items: center; gap: 5px;
    }

    /* Score ring */
    .score-ring-wrap { text-align: center; flex-shrink: 0; }
    .score-ring {
        width: 104px; height: 104px; border-radius: 50%;
        background: conic-gradient(rgba(255,255,255,.88) var(--pct, 0deg), rgba(255,255,255,.18) 0deg);
        display: flex; align-items: center; justify-content: center;
        margin: 0 auto 8px; transition: background 1.2s ease;
    }
    .score-ring-inner {
        width: 80px; height: 80px; border-radius: 50%;
        background: linear-gradient(140deg, #2e1065, #5b21b6);
        display: flex; flex-direction: column;
        align-items: center; justify-content: center;
    }
    .score-number {
        font-family: 'Sora', sans-serif; font-size: 28px; font-weight: 800;
        color: #fff; line-height: 1;
    }
    .score-denom { font-size: 10px; color: rgba(255,255,255,.55); }
    .score-label { color: rgba(255,255,255,.65); font-size: 11px; font-weight: 600; letter-spacing: .8px; text-transform: uppercase; }

    /* Section heading */
    .section-title {
        font-family: 'Sora', sans-serif; font-size: 11px; font-weight: 700;
        letter-spacing: 1.2px; text-transform: uppercase; color: var(--muted);
        margin: 0 0 14px; display: flex; align-items: center; gap: 8px;
    }
    .section-title::after { content: ''; flex: 1; height: 1px; background: var(--border); }

    /* Score cards */
    .scores-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
        gap: 14px; margin-bottom: 28px;
    }
    .score-card {
        background: #fff; border-radius: var(--radius);
        border: 1.5px solid var(--border); padding: 20px 22px;
        transition: transform .18s, box-shadow .18s;
    }
    .score-card:hover { transform: translateY(-2px); box-shadow: var(--shadow); }
    .sc-top { display: flex; align-items: center; margin-bottom: 14px; gap: 10px; }
    .sc-index {
        width: 28px; height: 28px; border-radius: 8px;
        background: var(--brand-light); color: var(--brand-mid);
        font-family: 'Sora', sans-serif; font-size: 12px; font-weight: 700;
        display: flex; align-items: center; justify-content: center; flex-shrink: 0;
    }
    .sc-name {
        font-family: 'Sora', sans-serif; font-size: 13.5px; font-weight: 700;
        color: var(--ink); flex: 1; line-height: 1.3;
    }
    .sc-badge { font-size: 13px; font-weight: 700; padding: 3px 12px; border-radius: 50px; }
    .sc-badge.high { background: var(--green-bg); color: var(--green); }
    .sc-badge.mid  { background: var(--amber-bg); color: var(--amber); }
    .sc-badge.low  { background: var(--red-bg);   color: var(--red);   }
    .sc-bar-wrap { background: #f0edf8; border-radius: 6px; height: 6px; overflow: hidden; margin-bottom: 12px; }
    .sc-bar-fill { height: 100%; border-radius: 6px; width: 0; transition: width 1.1s cubic-bezier(.4,0,.2,1); }
    .sc-bar-fill.high { background: linear-gradient(90deg,#059669,#34d399); }
    .sc-bar-fill.mid  { background: linear-gradient(90deg,#f59e0b,#fbbf24); }
    .sc-bar-fill.low  { background: linear-gradient(90deg,#dc2626,#f87171); }
    .sc-comment { font-size: 13px; color: var(--muted); line-height: 1.6; margin: 0; }

    /* Assessment */
    .assessment-card {
        background: #fff; border-radius: var(--radius);
        border: 1.5px solid var(--border); padding: 28px 30px; margin-bottom: 20px;
    }
    .assessment-card h5 {
        font-family: 'Sora', sans-serif; font-size: 15px; font-weight: 700;
        color: var(--ink); margin: 0 0 16px; display: flex; align-items: center; gap: 10px;
    }
    .ai-chip {
        background: var(--brand-light); color: var(--brand-mid);
        font-size: 10px; font-weight: 700; letter-spacing: .5px;
        text-transform: uppercase; padding: 3px 10px; border-radius: 50px;
    }
    .assessment-text {
        font-size: 14px; line-height: 1.8; color: #374151;
        background: #f9f7ff; border-left: 3px solid var(--brand-mid);
        border-radius: 0 8px 8px 0; padding: 16px 20px; margin: 0;
        white-space: pre-wrap;
    }

    /* Two column cards */
    .two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 32px; }
    @media (max-width: 640px) { .two-col { grid-template-columns: 1fr; } .report-hero { padding: 24px 20px; } }
    .list-card {
        background: #fff; border-radius: var(--radius);
        border: 1.5px solid var(--border); padding: 24px 26px;
    }
    .list-card-title {
        font-family: 'Sora', sans-serif; font-size: 14px; font-weight: 700;
        margin: 0 0 14px; display: flex; align-items: center; gap: 8px;
    }
    .list-card-title.green { color: var(--green); }
    .list-card-title.red   { color: #e05252; }
    .list-card ul { list-style: none; padding: 0; margin: 0; }
    .list-card ul li {
        font-size: 13.5px; color: #374151; padding: 8px 0;
        border-bottom: 1px solid #f3f4f6;
        display: flex; align-items: flex-start; gap: 9px; line-height: 1.5;
    }
    .list-card ul li:last-child { border-bottom: none; }
    .list-card ul li::before {
        content: ''; display: block; width: 7px; height: 7px;
        border-radius: 50%; flex-shrink: 0; margin-top: 6px;
    }
    .list-card.strengths ul li::before { background: var(--green); }
    .list-card.areas     ul li::before { background: #e05252; }
    .list-card .empty-note { font-size: 13px; color: #9ca3af; font-style: italic; }
</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div style="max-width: 960px; margin: 0 auto; padding: 28px 20px 60px;">

    <a href="javascript:history.back()" class="btn-back">
        <i class="fas fa-arrow-left" style="font-size:11px;"></i> Back to Applicants
    </a>

    <!-- ── Hero ── -->
    <div class="report-hero">
        <div class="hero-layout">
            <div style="display:flex; align-items:center; gap:18px; flex:1; min-width:200px;">
                <div class="candidate-avatar" id="avatarEl">
                    <asp:Literal ID="litCandidateName" runat="server" />
                </div>
                <div>
                    <h2 class="candidate-name" id="hName" runat="server"></h2>
                    <p class="candidate-email"><asp:Literal ID="litCandidateEmail" runat="server" /></p>
                    <div class="meta-pills">
                        <span class="meta-pill"><i class="fas fa-briefcase"></i><asp:Literal ID="litRole" runat="server" /></span>
                        <span class="meta-pill"><i class="fas fa-layer-group"></i><asp:Literal ID="litLevel" runat="server" /></span>
                        <span class="meta-pill"><i class="fas fa-tag"></i><asp:Literal ID="litType" runat="server" /></span>
                        <span class="meta-pill"><i class="fas fa-code"></i><asp:Literal ID="litTechStack" runat="server" /></span>
                        <span class="meta-pill"><i class="far fa-calendar-alt"></i><asp:Literal ID="litDate" runat="server" /></span>
                    </div>
                </div>
            </div>
            <div class="score-ring-wrap">
                <div class="score-ring" id="scoreRing">
                    <div class="score-ring-inner">
                        <span class="score-number"><asp:Literal ID="litTotalScore" runat="server" /></span>
                        <span class="score-denom">/ 100</span>
                    </div>
                </div>
                <div class="score-label">Overall Score</div>
            </div>
        </div>
    </div>

    <!-- ── Category breakdown ── -->
    <p class="section-title"><i class="fas fa-chart-bar" style="color:var(--brand-mid)"></i> Category Breakdown</p>
    <div class="scores-grid">
        <asp:Literal ID="litScoreCards" runat="server" />
    </div>

    <!-- ── AI Assessment ── -->
    <p class="section-title"><i class="fas fa-robot" style="color:var(--brand-mid)"></i> AI Assessment</p>
    <div class="assessment-card">
        <h5><i class="fas fa-file-alt" style="color:var(--brand-mid)"></i> Final Evaluation <span class="ai-chip">AI Generated</span></h5>
        <p class="assessment-text"><asp:Literal ID="litFinalAssessment" runat="server" /></p>
    </div>

    <!-- ── Strengths & Areas ── -->
    <p class="section-title"><i class="fas fa-balance-scale" style="color:var(--brand-mid)"></i> Candidate Summary</p>
    <div class="two-col">
        <div class="list-card strengths">
            <p class="list-card-title green"><i class="fas fa-check-circle"></i> Strengths</p>
            <ul><asp:Literal ID="litStrengths" runat="server" /></ul>
        </div>
        <div class="list-card areas">
            <p class="list-card-title red"><i class="fas fa-exclamation-circle"></i> Areas for Improvement</p>
            <ul><asp:Literal ID="litAreas" runat="server" /></ul>
        </div>
    </div>

</div>

<script>
window.addEventListener('load', function () {
    // Avatar: first letter of candidate name
    var av = document.getElementById('avatarEl');
    if (av) {
        var t = av.innerText.trim();
        av.innerHTML = t.charAt(0).toUpperCase();
    }

    // Animate score progress bars
    document.querySelectorAll('.sc-bar-fill').forEach(function (el) {
        var s = parseInt(el.getAttribute('data-score') || '0');
        setTimeout(function () { el.style.width = Math.min(s, 100) + '%'; }, 350);
    });

    // Animate score ring conic gradient
    var ring = document.getElementById('scoreRing');
    if (ring) {
        var numEl = ring.querySelector('.score-number');
        var score = numEl ? parseInt(numEl.innerText) || 0 : 0;
        ring.style.setProperty('--pct', '0deg');
        setTimeout(function () {
            ring.style.setProperty('--pct', (score / 100 * 360) + 'deg');
            ring.style.transition = 'background 1.2s ease';
        }, 200);
    }
});
</script>
</asp:Content>
