Development Log
Each line records one short file change.
IntelliJob/GeminiService.cs: added updated resume rewrite output to the resume enhancer response.
IntelliJob/ApplicationDataStore.cs: added application snapshot and saved report storage.
IntelliJob/Models/ResumeEnhancementModels.cs: added saved report and application resume selection models.
IntelliJob/User/ResumeEnhancer.aspx: changed the preview to show the enhanced resume and added percent labels.
IntelliJob/User/ResumeEnhancer.aspx.cs: saved and reloaded resume reports from application history.
IntelliJob/User/JobDetails.aspx: added optional resume upload for each job application.
IntelliJob/User/JobDetails.aspx.cs: saved a per-application resume snapshot during apply.
IntelliJob/User/Profile.aspx: added a resume removal action.
IntelliJob/User/Profile.aspx.cs: deleted the stored resume file and cleared the profile record.
IntelliJob/User/JobApplications.aspx: added the job applications dashboard.
IntelliJob/User/JobApplications.aspx.cs: listed applications and linked saved resume reports.
IntelliJob/User/ApplicationDetails.aspx: added the application detail page.
IntelliJob/User/ApplicationDetails.aspx.cs: showed job details, saved resume reports, and interview feedbacks.
IntelliJob/User/InterviewHistory.aspx.cs: added job-specific interview history filtering.
IntelliJob/User/Home.aspx: added a job applications shortcut above featured jobs.
IntelliJob/User/JobListing.aspx: added a job applications shortcut above the filters.
IntelliJob/User/Interview.aspx: refreshed the interview page styling without changing interview logic.
IntelliJob/User/UserMaster.Master: made the profile and logout buttons visible on smaller screens.
IntelliJob/User/JobDetails.aspx: tightened the upload area, added apply confirmation, and centered the success message.
IntelliJob/User/Profile.aspx: compacted the remove resume button and kept it after the view link.
IntelliJob/ResumePdfExporter.cs: added a lightweight PDF exporter for saved enhanced resume reports.
IntelliJob/User/ApplicationDetails.aspx.cs: added a PDF download link for saved application reports.
IntelliJob/User/Register.aspx: added optional resume upload during registration.
IntelliJob/User/ResumeEnhancer.aspx: added resume update and applications shortcuts.
IntelliJob/User/JobDetails.aspx, IntelliJob/User/ApplicationResumeBuild.aspx, IntelliJob/ApplicationDataStore.cs: added the application resume draft flow and editor.
IntelliJob/ApplicationDataStore.cs: kept identity fields in the application draft snapshot text.
IntelliJob/User/ResumeEnhancer.aspx, IntelliJob/User/ResumeEnhancer.aspx.cs: replaced the flat preview with an editable structured resume editor and save targets.
IntelliJob/User/ResumeEnhancer.aspx, IntelliJob/User/ResumeEnhancer.aspx.cs: kept the enhancer body visible on edit postbacks and reorganized the report layout.
IntelliJob/User/JobDetails.aspx.cs: prevented duplicate applies, validated candidate email, and queued interview invitations.
IntelliJob/User/InterviewAccess.aspx.cs: deferred interview-code consumption until the live interview starts.
IntelliJob/User/TakeInterview.aspx.cs: blocked direct re-entry after access is consumed and kept pre-call retries available.
IntelliJob/User/ConsumeInterviewAccess.ashx.cs: added a handler to revoke invitation access when the call actually begins.
IntelliJob/User/Interview.aspx: removed the persistent setup toggle so the button resets on reload.
IntelliJob/User/JobDetails.aspx.cs: resolved company logos from photos, images, or a real No_image fallback.
IntelliJob/Company/PostJob.aspx.cs: changed missing company-logo default to Images/No_image.png.
IntelliJob/Company/EditJobDetails.aspx.cs: changed missing company-logo default to Images/No_image.png.
IntelliJob/User/Register.aspx.cs, IntelliJob/User/Profile.aspx.cs, IntelliJob/User/ResumeBuild.aspx.cs: added resume import, structured resume storage, and profile editing updates.
IntelliJob/User/ResumeBuild.aspx, IntelliJob/User/Profile.aspx: capped resume textarea height and hid the profile import block when a resume is already stored.
IntelliJob/User/ResumeBuild.aspx.cs: opened the resume-path lookup connection before update so the builder can save edits.
IntelliJob/Models/ResumeEnhancementModels.cs: added UpdatedResumeStructuredJson field to preserve edited resume data without re-parsing.
IntelliJob/User/ResumeEnhancer.aspx.cs: fixed preview sync by storing structured JSON, removed BindEditablePreview call after save, BuildEditableDocument now tries JSON before text parsing, simplified edit toggle to enter-only, ApplyPreviewEditorState hides Edit button when editing.
IntelliJob/User/ResumeEnhancer.aspx: removed Lock Preview concept, renamed save panel to Update Resume with single Update button, updated instruction text.
IntelliJob/User/ResumeEnhancer.aspx: removed radio selection, save target auto-detected from hfLoadedResumeSource, commented out hero Update Resume button, added editor-section class and edit-mode CSS to hide report sections during editing.
IntelliJob/User/ResumeEnhancer.aspx.cs: replaced rblEnhSaveTarget logic with hfLoadedResumeSource auto-detection, removed radio references from BindEditablePreview, added JS class toggle for edit-mode section hiding.
IntelliJob/User/ResumeEnhancer.aspx.designer.cs: removed rblEnhSaveTarget field reference.
IntelliJob/Models/ResumeProfileModels.cs: added LinkedInUrl and PortfolioUrl optional fields to ResumeProfileDocument.
IntelliJob/User/ResumeEnhancer.aspx: added Resume Source hero chip, LinkedIn/Portfolio form fields, moved Export PDF button beside Edit Preview.
IntelliJob/User/ResumeEnhancer.aspx.cs: wired LinkedIn/Portfolio in PopulateEditablePreview, BuildDocumentFromForm, MergeDocumentDefaults, BuildStructuredResumeText, ApplyPreviewEditorState; added litResumeSource in RenderReport; added btnExportResumePdf_Click handler for template-based resume PDF export.
IntelliJob/User/ResumeEnhancer.aspx.designer.cs: added litResumeSource, txtEnhLinkedIn, txtEnhPortfolio, btnExportResumePdf field declarations.
IntelliJob/ResumePdfExporter.cs: added BuildResume() method for one-page template-based resume PDF with icon XObjects, section headings with separator lines, clickable hyperlink annotations, and proper A4 layout matching MY_RESUME/resume.docx template.
IntelliJob/assets/img/resume-icons/: copied 6 contact icons (email, phone, location, linkedin, github, portfolio) from MY_RESUME for PDF embedding.
IntelliJob/GeminiService.cs: updated GenerateQuestionsAsync to accept optional resumeText for tailored questions.
IntelliJob/User/JobDetails.aspx.cs, IntelliJob/Company/ShorlistedCandidates.aspx.cs, IntelliJob/User/Interview.aspx.cs: integrated ResumeTextExtractor to fetch specific application or profile resume text and pass it to Gemini for questions generation.
IntelliJob/User/TakeInterview.aspx.cs: confirmed company-led interviews are securely gated by AuthorizedInterviewId and IsPasswordUsed, ensuring one-time conduction.
IntelliJob/User/ResumeEnhancer.aspx.cs: confirmed enhancement uses specific applied resume and caches report to ensure one-time generation per application.
IntelliJob/User/ResumeEnhancer.aspx, IntelliJob/User/ResumeEnhancer.aspx.cs: replaced plain score texts with SVG Score Circle visual components for overall, ATS, semantic, and keyword matches.
IntelliJob/User/InterviewFeedback.aspx, IntelliJob/User/InterviewFeedback.aspx.cs: replaced plain progress bars with SVG Score Gauge components for categorical performance scores.
IntelliJob/User/TakeInterview.aspx: added a fallback Text Chat mode to allow users to conduct interviews sequentially via text without using VAPI tokens, seamlessly hooking into the existing transcript submission flow.
