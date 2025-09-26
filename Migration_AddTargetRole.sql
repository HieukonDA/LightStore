-- Migration: Add TargetRole column to Notifications table
-- Date: 2025-09-26
-- Description: Thêm cột TargetRole để phân biệt rõ ràng thông báo cho admin vs customer

-- Add TargetRole column
ALTER TABLE [Notifications] 
ADD [TargetRole] NVARCHAR(20) NOT NULL DEFAULT 'admin';

-- Create index for better performance
CREATE INDEX IX_Notifications_TargetRole ON [Notifications] ([TargetRole]);

-- Create composite index for user queries
CREATE INDEX IX_Notifications_UserId_TargetRole_IsRead ON [Notifications] ([UserId], [TargetRole], [IsRead]);

-- Update existing records based on RedirectUrl patterns
UPDATE [Notifications] 
SET [TargetRole] = CASE 
    WHEN [RedirectUrl] LIKE '%/admin/%' THEN 'admin'
    WHEN [RedirectUrl] LIKE '%/orders/%' AND [RedirectUrl] NOT LIKE '%/admin/%' THEN 'customer'
    WHEN [Type] = 'promotion' THEN 'customer'
    ELSE 'admin'
END
WHERE [TargetRole] = 'admin'; -- Only update default values

-- Add constraint to ensure valid values
ALTER TABLE [Notifications] 
ADD CONSTRAINT CK_Notifications_TargetRole 
CHECK ([TargetRole] IN ('admin', 'customer', 'all'));

GO

-- Create stored procedure to get notifications by role
CREATE OR ALTER PROCEDURE sp_GetNotificationsByRole
    @UserId INT = NULL,
    @TargetRole NVARCHAR(20),
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @IsRead BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- Main query
    SELECT 
        [Id],
        [UserId],
        [TargetRole],
        [Type],
        [Title],
        [Content],
        [ReferenceId],
        [RedirectUrl],
        [IsRead],
        [Priority],
        [CreatedAt],
        [ReadAt],
        [ExpiresAt],
        [Metadata],
        COUNT(*) OVER() as TotalCount
    FROM [Notifications]
    WHERE 
        ([TargetRole] = @TargetRole OR [TargetRole] = 'all')
        AND (@UserId IS NULL OR [UserId] = @UserId OR [UserId] IS NULL)
        AND (@IsRead IS NULL OR [IsRead] = @IsRead)
        AND ([ExpiresAt] IS NULL OR [ExpiresAt] > GETUTCDATE())
    ORDER BY 
        [Priority] DESC,
        [CreatedAt] DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;

GO

-- Create stored procedure to get notification stats by role  
CREATE OR ALTER PROCEDURE sp_GetNotificationStatsByRole
    @UserId INT = NULL,
    @TargetRole NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) as TotalCount,
        SUM(CASE WHEN [IsRead] = 0 THEN 1 ELSE 0 END) as UnreadCount,
        SUM(CASE WHEN CAST([CreatedAt] AS DATE) = CAST(GETUTCDATE() AS DATE) THEN 1 ELSE 0 END) as TodayCount,
        SUM(CASE WHEN [Priority] IN ('high', 'urgent') AND [IsRead] = 0 THEN 1 ELSE 0 END) as HighPriorityCount
    FROM [Notifications]
    WHERE 
        ([TargetRole] = @TargetRole OR [TargetRole] = 'all')
        AND (@UserId IS NULL OR [UserId] = @UserId OR [UserId] IS NULL)
        AND ([ExpiresAt] IS NULL OR [ExpiresAt] > GETUTCDATE());
END;

GO

-- Sample data update
PRINT 'Migration completed successfully. TargetRole column added with indexes and constraints.';
PRINT 'Updated ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' existing records.';