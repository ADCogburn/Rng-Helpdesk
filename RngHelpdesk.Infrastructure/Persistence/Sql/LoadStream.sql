select *
from eventstore.event_store
where "StreamType" = @streamType
  and "StreamId" = @streamId
order by "StreamVersion";
