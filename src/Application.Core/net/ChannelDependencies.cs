using client.processor.npc;
using service;

namespace net;

public record ChannelDependencies(NoteService noteService, FredrickProcessor fredrickProcessor);