from enum import Enum

from pydantic import BaseModel, Field, ConfigDict
from pydantic.alias_generators import to_camel


class MessageRole(str, Enum):
    user = "user"
    assistant = "assistant"
    system = "system"


class ChatMessage(BaseModel):
    role: MessageRole
    content: str


class CamelModel(BaseModel):
    model_config = ConfigDict(
        alias_generator=to_camel,
        populate_by_name=True,
    )


class AiServiceRequest(CamelModel):
    session_id: str
    user_message: str
    history: list[ChatMessage] = Field(default_factory=list)
    current_summary: str | None = None
    relevant_memories: list[str] = Field(default_factory=list)
    active_habits: list[str] = Field(default_factory=list)
    task: str = "chat"
    persona: str = "habit coach and personal assistant"
    tone: str = "concise and supportive"


class AiServiceResponse(CamelModel):
    answer: str
    prompt_tokens: int


class SummaryRequest(CamelModel):
    session_id: str
    history: list[ChatMessage] = Field(default_factory=list)
    current_summary: str | None = None
    existing_facts: list[str] = Field(default_factory=list)


class UpdatedFactItem(CamelModel):
    id: int
    updated: str


class SummaryResponse(CamelModel):
    summary: str
    new_facts: list[str] = Field(default_factory=list)
    updated_facts: list[UpdatedFactItem] = Field(default_factory=list)


class EmbeddingRequest(CamelModel):
    text: str


class EmbeddingResponse(CamelModel):
    embedding: list[float]
