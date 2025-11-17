/****** Object:  Trigger [dbo].[trg_PatientStatusInactive]    Script Date: 16-11-2025 17:03:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TRIGGER [dbo].[trg_PatientStatusInactive]
ON [dbo].[Patient]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PatientSMSQueue (PatientId, PatientNumber, MobileNo)
    SELECT 
        i.Id AS PatientId,
        i.PatientNumber,
        u.MobileNo
    FROM inserted i
    INNER JOIN deleted d ON i.Id = d.Id
    LEFT JOIN dbo.Users u ON i.UserId = u.Id
    WHERE 
        -- Status changed EXACTLY to 'InActive'
        i.Status = 'InActive'
        AND (d.Status IS NULL OR d.Status <> 'InActive');
END
GO

ALTER TABLE [dbo].[Patient] ENABLE TRIGGER [trg_PatientStatusInactive]
GO


