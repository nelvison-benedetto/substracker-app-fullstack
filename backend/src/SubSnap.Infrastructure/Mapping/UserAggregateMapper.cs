//using SubSnap.Core.Domain.Aggregates;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SubSnap.Infrastructure.Mapping;

//public static class UserAggregateMapper
//{
//    public static UserAggregate ToDomain(Infrastructure.Persistence.Scaffold.User entity)
//    {
//        return new UserAggregate(  //il type(here utilizzo le rules e validations)
//            UserMapper.ToDomain(entity),
//            entity.Subscription.Select(SubscriptionMapper.ToDomain),
//            entity.SharedLink.Select(SharedLinkMapper.ToDomain)
//        );
//    }

//}
