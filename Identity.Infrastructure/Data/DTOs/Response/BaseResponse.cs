﻿namespace Identity.Infrastructure.Data.DTOs.Response
{
    public class BaseResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}