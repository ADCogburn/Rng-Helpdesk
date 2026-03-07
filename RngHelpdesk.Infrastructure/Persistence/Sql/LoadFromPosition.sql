select *
from eventstore.event_store
where "GlobalPosition" > @position
order by "GlobalPosition";
