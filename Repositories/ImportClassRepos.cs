using Bug_Tracking_System.Models;
using Bug_Tracking_System.Repositories.Interfaces;
using ClosedXML.Excel;

namespace Bug_Tracking_System.Repositories
{
    public class ImportClassRepos : IImportRepos
    {
        private readonly DbBug _dbBug;
        private readonly IBugRepos _bug;
        private readonly IAccountRepos _acc;

        public ImportClassRepos(DbBug dbBug, IBugRepos bug, IAccountRepos acc)
        {
            _dbBug = dbBug;
            _bug = bug;
            _acc = acc;
        }



        public byte[] GenerateSampleBugExcel(bool bug)
        {
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Sample");

            // Headers
            sheet.Cell(1, 1).Value = "Title";
            sheet.Cell(1, 2).Value = "Description";
            sheet.Cell(1, 3).Value = "Severity";    // Dropdown
            sheet.Cell(1, 4).Value = "Priority";    // Dropdown
            sheet.Cell(1, 5).Value = "Status";      // Dropdown
            sheet.Cell(1, 6).Value = "Created By";
            sheet.Cell(1, 7).Value = "Created Date";
            sheet.Cell(1, 8).Value = "Project";

            // Static dropdown values
            var priorities = new List<string> { "Highest", "High", "Medium", "Low", "Lowest" };
            var severities = new List<string> { "Critical", "Major", "Minor", "Low" };

            // Dynamic dropdown values
            var statuses = _dbBug.BugStatuses.Select(s => s.StatusName).ToList();
            var projects = _dbBug.Projects.Select(p => p.ProjectName).ToList();

            // Hidden sheet for dropdowns
            var hiddenSheet = workbook.Worksheets.Add("Dropdowns");

            // Fill Priority in Column A
            for (int i = 0; i < priorities.Count; i++)
                hiddenSheet.Cell(i + 1, 1).Value = priorities[i]; // A

            // Fill Severity in Column B
            for (int i = 0; i < severities.Count; i++)
                hiddenSheet.Cell(i + 1, 2).Value = severities[i]; // B

            // Fill Status in Column C
            for (int i = 0; i < statuses.Count; i++)
                hiddenSheet.Cell(i + 1, 3).Value = statuses[i]; // C

            // Fill Projects in Column D ✅
            for (int i = 0; i < projects.Count; i++)
                hiddenSheet.Cell(i + 1, 4).Value = projects[i]; // D

            hiddenSheet.Hide();

            // Apply Severity dropdown (C2:C100)
            var severityRange = sheet.Range("C2:C100");
            var severityValidation = severityRange.CreateDataValidation();
            severityValidation.List(hiddenSheet.Range($"B1:B{severities.Count}"));

            // Apply Priority dropdown (D2:D100)
            var priorityRange = sheet.Range("D2:D100");
            var priorityValidation = priorityRange.CreateDataValidation();
            priorityValidation.List(hiddenSheet.Range($"A1:A{priorities.Count}"));

            // Apply Status dropdown (E2:E100)
            var statusRange = sheet.Range("E2:E100");
            var statusValidation = statusRange.CreateDataValidation();
            statusValidation.List(hiddenSheet.Range($"C1:C{statuses.Count}"));

            // Apply Project dropdown (H2:H100) ✅ Corrected range
            var projectRange = sheet.Range("H2:H100");
            var projectValidation = projectRange.CreateDataValidation();
            projectValidation.List(hiddenSheet.Range($"D1:D{projects.Count}")); // ✅ Use Column D now

            // Save to memory
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }


        //Report generation
        public byte[] GenerateStatusReport(List<ImportStatusModel> results)
        {
            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Import Status");

                // Headers
                sheet.Cell("A1").Value = "Name";
                sheet.Cell("B1").Value = "Status";
                sheet.Cell("C1").Value = "Message";

                int row = 2;
                foreach (var result in results)
                {
                    sheet.Cell(row, 1).Value = result.Name;
                    sheet.Cell(row, 2).Value = result.Status;
                    sheet.Cell(row, 3).Value = result.Message;
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> BugsExcelImport(IFormFile file)
        {
            var resultList = new List<ImportStatusModel>();
            var imageDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BugAttachments");

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var sheet = workbook.Worksheet(1);
                    var rows = sheet.RowsUsed().Skip(1); // Skip header
                    int rowIndex = 1;

                    foreach (var row in rows)
                    {
                        rowIndex++; // Start from 2 (because row 1 is header)

                        var title = row.Cell(1).GetString().Trim();
                        var description = row.Cell(2).GetString().Trim();
                        var severity = row.Cell(3).GetString().Trim();
                        var priority = row.Cell(4).GetString().Trim();
                        var statusName = row.Cell(5).GetString().Trim();
                        var createdByUsername = row.Cell(6).GetString().Trim();
                        var createdDateStr = row.Cell(7).GetString().Trim();
                        var projectName = row.Cell(8).GetString().Trim();

                        var statusReport = new ImportStatusModel
                        {
                            Name = title,
                            Status = "Pending",
                            Message = ""
                        };

                        try
                        {
                            var validationErrors = new List<string>();

                            if (string.IsNullOrWhiteSpace(title)) validationErrors.Add("Title is missing.");
                            if (string.IsNullOrWhiteSpace(description)) validationErrors.Add("Description is missing.");
                            if (string.IsNullOrWhiteSpace(severity)) validationErrors.Add("Severity is missing.");
                            if (string.IsNullOrWhiteSpace(priority)) validationErrors.Add("Priority is missing.");
                            if (string.IsNullOrWhiteSpace(statusName)) validationErrors.Add("Status is missing.");
                            if (string.IsNullOrWhiteSpace(createdByUsername)) validationErrors.Add("CreatedBy (username) is missing.");
                            if (string.IsNullOrWhiteSpace(projectName)) validationErrors.Add("Project is missing.");

                            var status = _dbBug.BugStatuses.FirstOrDefault(s => s.StatusName == statusName);
                            var project = _dbBug.Projects.FirstOrDefault(p => p.ProjectName == projectName);
                            var user = _dbBug.Users.FirstOrDefault(u => u.UserName == createdByUsername);

                            if (status == null) validationErrors.Add($"Status '{statusName}' does not exist.");
                            if (project == null) validationErrors.Add($"Project '{projectName}' does not exist.");
                            if (user == null) validationErrors.Add($"User '{createdByUsername}' does not exist.");

                            if (validationErrors.Any())
                            {
                                statusReport.Status = "Failed";
                                statusReport.Message = string.Join(" ", validationErrors);
                                resultList.Add(statusReport);
                                continue;
                            }

                            if (!DateTime.TryParse(createdDateStr, out var createdDate))
                                createdDate = DateTime.Now;

                            var bug = new Bug
                            {
                                Title = title,
                                Description = description,
                                Severity = severity,
                                Priority = priority,
                                StatusId = status.StatusId,
                                CreatedBy = user.UserId,
                                CreatedDate = createdDate,
                                ProjectId = project.ProjectId
                            };

                            _dbBug.Bugs.Add(bug);
                            await _dbBug.SaveChangesAsync();

                            // 🖼 Match multiple images for this row/bug
                            var matchingImages = Directory.GetFiles(imageDir)
                                                          .Where(f =>
                                                          {
                                                              var fileName = Path.GetFileNameWithoutExtension(f);
                                                              return fileName.StartsWith($"{rowIndex}_"); // match like "2_", "2_a", etc.
                                                          })
                                                          .ToList();


                            foreach (var imageFile in matchingImages)
                            {
                                _dbBug.Attachments.Add(new Attachment
                                {
                                    BugId = bug.BugId,
                                    FilePath = $"/BugAttachments/{Path.GetFileName(imageFile)}",
                                    UploadedDate = DateTime.Now
                                });
                            }

                            await _dbBug.SaveChangesAsync();

                            statusReport.Status = "Success";
                            statusReport.Message = "Imported successfully.";
                        }
                        catch (Exception ex)
                        {
                            statusReport.Status = "Failed";
                            statusReport.Message = $"Exception: {ex.Message}";
                        }

                        resultList.Add(statusReport);
                    }
                }
            }

            return GenerateStatusReport(resultList);
        }

       
    }
}
