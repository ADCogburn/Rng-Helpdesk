select "CurrentVersion"
from eventstore.event_streams
where "StreamType" = @streamType
  and "StreamId" = @streamId
for update;
