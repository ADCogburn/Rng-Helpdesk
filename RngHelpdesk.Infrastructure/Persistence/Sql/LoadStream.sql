select *
from event_store
where stream_type = @streamType
  and stream_id = @streamId
order by stream_version;