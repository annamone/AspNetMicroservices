using MediatR;
using Ordering.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Ordering.Application.Features.Queries.GetOrderList
{
	public class GetOrdersListQuery : IRequest<List<OrdersVm>>
	{
		public string UserName { get; set; }

		public GetOrdersListQuery(string userName)
		{
			UserName = userName ?? throw new ArgumentNullException(nameof(userName));
		}
	}
}
