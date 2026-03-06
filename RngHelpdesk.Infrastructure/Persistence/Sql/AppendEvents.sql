select current_version
from event_streams
where stream_type = @streamType
  and stream_id = @streamId
for update;