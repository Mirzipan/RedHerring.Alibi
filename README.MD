# Red Herring Alibi

![Nuget](https://img.shields.io/nuget/v/RedHerring.Alibi)

Simple update scheduler and ticker for your game.
Use this if you want to have better control when your updates get called and free up some performance.

## Scheduled Updates

The basic idea is that whatever callback you schedule, the only guarantee is that the callback will not be called before its due time.
There is, however, no guarantee that it will be called precisely when scheduled.
All time-related parameters are either a `TimeSpan` or a `long` in milliseconds.

### Constructor

```csharp
Scheduler(TimeSpan startingTime, long frameBudget);
```
`startingTime` - starting time for ticks
`frameBudget` - the maximum amount of time per one invoke of the `Tick` method

### Tick

```csharp
void Tick(TimeSpan time);
```
`time` - game time according to which scheduled updates should be invoked.

Starts calling scheduled updates until it exceeds the `frameBudget` specified within constructor.

### Dispose

```csharp
void Dispose();
```
Basic cleanup when getting rid of the scheduler.

### SetFrameBudget

```csharp
void SetFrameBudget(long frameBudget);
```
Sets the new frame budget. Useful when changing target framerate of the application during runtime.

### Schedule

```csharp
public delegate void DeferredUpdate(double elapsedTime);

IDisposable Schedule(DeferredUpdate update, long dueTime);
IDisposable ScheduleRepeating(DeferredUpdate update, long period);
```
The specified callback will be called after `dueTime` has elapsed, but not before.
Optionally, a recurring callback can be specified with a `period`.
The callback can be unscheduled by disposing of the `IDisposable` object.

_Notice: Callback will never be called within the same tick as it was scheduled._

### Unschedule

```csharp
void Unschedule(DeferredUpdate update);
```
The specified callback will be unregistered from scheduled updates.

### Clear

All scheduled updated will be unscheduled.

## Ticker

Calls registered updates each tick, regardless of time.

### Add

```csharp
public delegate void TickUpdate();

IDisposable Add(TickUpdate update);
IDisposable Add(TickUpdate update, int priority);
```
`priority` - priority with which the update is to be called (higher means sooner).

Adds the specified callback to the list of updates to be called during a tick.

### Remove

```csharp
bool Remove(TickUpdate update)
```