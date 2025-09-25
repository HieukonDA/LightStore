-- =============================================
-- Script: Tạo bảng Notifications cho hệ thống thông báo real-time
-- Tác giả: TheLightStore
-- Ngày tạo: 2025-09-25
-- Mô tả: Bảng lưu trữ thông báo cho Admin/Staff về đơn hàng, thanh toán, tồn kho
-- =============================================

USE [TheLightStore_DB] -- Thay tên database của bạn
GO

-- Kiểm tra và xóa bảng nếu đã tồn tại (chỉ dùng khi development)
-- IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL
-- DROP TABLE dbo.Notifications;
-- GO

-- Tạo bảng Notifications
CREATE TABLE [dbo].[Notifications] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    
    -- ID người nhận thông báo (NULL = broadcast cho tất cả Admin/Staff)
    [UserId] INT NULL,
    
    -- Loại thông báo
    [Type] NVARCHAR(50) NOT NULL DEFAULT 'system',
    
    -- Tiêu đề thông báo
    [Title] NVARCHAR(255) NOT NULL,
    
    -- Nội dung thông báo
    [Content] NVARCHAR(MAX) NOT NULL,
    
    -- ID tham chiếu (OrderId, ProductId, PaymentId,...)
    [ReferenceId] INT NULL,
    
    -- URL chuyển hướng khi click thông báo
    [RedirectUrl] NVARCHAR(500) NULL,
    
    -- Trạng thái đã đọc
    [IsRead] BIT NOT NULL DEFAULT 0,
    
    -- Mức độ ưu tiên
    [Priority] NVARCHAR(20) NOT NULL DEFAULT 'normal',
    
    -- Thời gian tạo
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    -- Thời gian đọc
    [ReadAt] DATETIME2(7) NULL,
    
    -- Thời gian hết hạn
    [ExpiresAt] DATETIME2(7) NULL,
    
    -- Metadata bổ sung (JSON format)
    [Metadata] NVARCHAR(MAX) NULL,
    
    -- Khóa chính
    CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED ([Id] ASC),
    
    -- Khóa ngoại tới bảng Users
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[Users]([Id]) ON DELETE SET NULL,
        
    -- Ràng buộc kiểm tra Type
    CONSTRAINT [CK_Notifications_Type] CHECK ([Type] IN ('order', 'payment', 'inventory', 'system', 'user')),
    
    -- Ràng buộc kiểm tra Priority
    CONSTRAINT [CK_Notifications_Priority] CHECK ([Priority] IN ('low', 'normal', 'high', 'urgent'))
);
GO

-- =============================================
-- Tạo các Index để tối ưu hiệu năng
-- =============================================

-- Index cho UserId để query nhanh thông báo của user
CREATE NONCLUSTERED INDEX [IX_Notifications_UserId] 
ON [dbo].[Notifications] ([UserId])
WHERE [UserId] IS NOT NULL;
GO

-- Index cho Type để lọc theo loại thông báo
CREATE NONCLUSTERED INDEX [IX_Notifications_Type] 
ON [dbo].[Notifications] ([Type]);
GO

-- Index cho IsRead để lấy thông báo chưa đọc
CREATE NONCLUSTERED INDEX [IX_Notifications_IsRead] 
ON [dbo].[Notifications] ([IsRead]);
GO

-- Index cho CreatedAt để sắp xếp theo thời gian
CREATE NONCLUSTERED INDEX [IX_Notifications_CreatedAt] 
ON [dbo].[Notifications] ([CreatedAt] DESC);
GO

-- Index cho Priority để lấy thông báo ưu tiên cao
CREATE NONCLUSTERED INDEX [IX_Notifications_Priority] 
ON [dbo].[Notifications] ([Priority]);
GO

-- Index cho ExpiresAt để xóa thông báo hết hạn
CREATE NONCLUSTERED INDEX [IX_Notifications_ExpiresAt] 
ON [dbo].[Notifications] ([ExpiresAt])
WHERE [ExpiresAt] IS NOT NULL;
GO

-- Index composite cho query thông báo của user theo thời gian
CREATE NONCLUSTERED INDEX [IX_Notifications_User_CreatedAt] 
ON [dbo].[Notifications] ([UserId], [CreatedAt] DESC)
INCLUDE ([Type], [Title], [IsRead], [Priority]);
GO

-- Index cho ReferenceId để tìm thông báo theo đối tượng tham chiếu
CREATE NONCLUSTERED INDEX [IX_Notifications_ReferenceId] 
ON [dbo].[Notifications] ([ReferenceId])
WHERE [ReferenceId] IS NOT NULL;
GO

-- =============================================
-- Thêm dữ liệu mẫu (Optional - có thể xóa trong production)
-- =============================================

INSERT INTO [dbo].[Notifications] ([UserId], [Type], [Title], [Content], [Priority], [Metadata])
VALUES 
    -- Thông báo broadcast cho tất cả admin
    (NULL, 'system', 'Hệ thống thông báo đã được kích hoạt', 'Hệ thống thông báo real-time qua SignalR đã sẵn sàng hoạt động.', 'normal', '{"source": "system_initialization"}'),
    
    -- Thông báo đơn hàng mẫu
    (NULL, 'order', 'Đơn hàng mới #ORD001', 'Khách hàng Nguyễn Văn A vừa đặt đơn hàng #ORD001 với giá trị 1.500.000đ', 'high', '{"orderId": 1, "customerName": "Nguyễn Văn A", "amount": 1500000}'),
    
    -- Thông báo thanh toán mẫu
    (NULL, 'payment', 'Thanh toán thành công #ORD001', 'Đơn hàng #ORD001 đã được thanh toán thành công số tiền 1.500.000đ', 'high', '{"orderId": 1, "paymentMethod": "momo", "amount": 1500000}'),
    
    -- Thông báo tồn kho mẫu
    (NULL, 'inventory', 'Cảnh báo hết hàng', 'Sản phẩm "Đèn LED 12W" chỉ còn 5 sản phẩm trong kho', 'urgent', '{"productId": 1, "productName": "Đèn LED 12W", "stock": 5}');
GO

-- =============================================
-- Stored Procedures hỗ trợ (Optional)
-- =============================================

-- Procedure đánh dấu thông báo đã đọc
CREATE PROCEDURE [dbo].[sp_MarkNotificationsAsRead]
    @UserId INT,
    @NotificationIds NVARCHAR(MAX) -- Danh sách ID cách nhau bởi dấu phẩy
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[Notifications] 
    SET [IsRead] = 1, [ReadAt] = GETUTCDATE()
    WHERE [Id] IN (SELECT value FROM STRING_SPLIT(@NotificationIds, ','))
      AND ([UserId] = @UserId OR [UserId] IS NULL);
END
GO

-- Procedure lấy thống kê thông báo
CREATE PROCEDURE [dbo].[sp_GetNotificationStats]
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Today DATE = CAST(GETUTCDATE() AS DATE);
    
    SELECT 
        COUNT(*) AS TotalCount,
        SUM(CASE WHEN [IsRead] = 0 THEN 1 ELSE 0 END) AS UnreadCount,
        SUM(CASE WHEN CAST([CreatedAt] AS DATE) = @Today THEN 1 ELSE 0 END) AS TodayCount,
        SUM(CASE WHEN [Priority] IN ('high', 'urgent') AND [IsRead] = 0 THEN 1 ELSE 0 END) AS HighPriorityCount
    FROM [dbo].[Notifications]
    WHERE ([UserId] = @UserId OR [UserId] IS NULL)
      AND ([ExpiresAt] IS NULL OR [ExpiresAt] > GETUTCDATE());
END
GO

-- Procedure xóa thông báo hết hạn (để chạy định kỳ)
CREATE PROCEDURE [dbo].[sp_CleanupExpiredNotifications]
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM [dbo].[Notifications]
    WHERE [ExpiresAt] IS NOT NULL 
      AND [ExpiresAt] < GETUTCDATE();
      
    SELECT @@ROWCOUNT AS DeletedCount;
END
GO

-- =============================================
-- Tạo Job để cleanup thông báo hết hạn (Optional)
-- =============================================
/*
-- Tạo SQL Server Agent Job để chạy cleanup hàng ngày
USE [msdb]
GO

EXEC dbo.sp_add_job
    @job_name = N'Cleanup Expired Notifications',
    @enabled = 1,
    @description = N'Xóa các thông báo đã hết hạn hàng ngày';

EXEC dbo.sp_add_jobstep
    @job_name = N'Cleanup Expired Notifications',
    @step_name = N'Delete Expired',
    @command = N'EXEC [TheLightStore_DB].[dbo].[sp_CleanupExpiredNotifications]',
    @database_name = N'TheLightStore_DB';

EXEC dbo.sp_add_schedule
    @schedule_name = N'Daily at 2 AM',
    @freq_type = 4, -- Daily
    @freq_interval = 1,
    @active_start_time = 020000; -- 2:00 AM

EXEC dbo.sp_attach_schedule
    @job_name = N'Cleanup Expired Notifications',
    @schedule_name = N'Daily at 2 AM';

EXEC dbo.sp_add_jobserver
    @job_name = N'Cleanup Expired Notifications';
GO
*/

-- =============================================
-- Kiểm tra kết quả
-- =============================================

-- Xem cấu trúc bảng
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Notifications'
ORDER BY ORDINAL_POSITION;

-- Xem dữ liệu mẫu
SELECT TOP 10 * FROM [dbo].[Notifications] ORDER BY [CreatedAt] DESC;

-- Xem các index đã tạo
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    STRING_AGG(c.name, ', ') AS Columns
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE i.object_id = OBJECT_ID('dbo.Notifications')
  AND i.type > 0 -- Exclude heap
GROUP BY i.name, i.type_desc
ORDER BY i.name;

PRINT 'Bảng Notifications đã được tạo thành công!'
PRINT 'Hệ thống thông báo real-time sẵn sàng hoạt động.'