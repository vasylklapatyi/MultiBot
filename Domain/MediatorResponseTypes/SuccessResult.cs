﻿namespace Domain
{
    public class SuccessResult : IResponse
	{
		public SuccessResult()
		{
			Succeded = true;
		}
		public bool Succeded { get; set; }
	}
}
