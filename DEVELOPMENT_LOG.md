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
