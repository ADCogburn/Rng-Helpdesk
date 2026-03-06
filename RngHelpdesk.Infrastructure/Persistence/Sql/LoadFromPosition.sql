select *
from event_store
where global_position > @position
order by global_position;