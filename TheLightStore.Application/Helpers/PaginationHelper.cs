using System;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Constants;

namespace TheLightStore.Application.Helpers;

public class PaginationHelper
{
    public static Pagination CreatePagination(long totalRecords, int pageSize, int pageNumber)
    {
        if (pageSize <= 0) pageSize = Numbers.Pagination.DefaultPageSize;

        var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

        int perPage = totalPages <= pageSize ? totalPages : pageSize;

        int? nextPage = null;
        int? prevPage = null;

        // Xử lý trường hợp đặc biệt khi không có bản ghi nào
        if (totalPages == 0)
        {
            pageNumber = 0; // Đặt trang hiện tại là 0
        }
        else
        {
            // Đảm bảo pageNumber nằm trong khoảng hợp lệ
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > totalPages) pageNumber = totalPages;

            // Tính toán trang tiếp theo và trang trước
            nextPage = (pageNumber < totalPages) ? pageNumber + 1 : null;
            prevPage = (pageNumber > 1) ? pageNumber - 1 : null;
        }

        // Trả về đối tượng Pagination
        return new Pagination
        {
            TotalRecords = (int)totalRecords,
            TotalPages = totalPages,
            CurrentPage = pageNumber,
            PerPage = pageSize,
            NextPage = nextPage,
            PrevPage = prevPage
        };
    }
}
