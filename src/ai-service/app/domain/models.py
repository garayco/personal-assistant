from enum import Enum

from pydantic import BaseModel, Field


class MessageRole(str, Enum):
    user = "user"
    assistant = "assistant"
    system = "system"


class ChatMessage(BaseModel):
    role: MessageRole
    content: str


class AiServiceRequest(BaseModel):
    session_id: str = Field(alias="sessionId")
    user_message: str = Field(alias="userMessage")
    history: list[ChatMessage] = Field(default_factory=list)
    current_summary: str | None = Field(alias="currentSummary", default=None)
    task: str = "chat"
    persona: str = "personal assistant"
    tone: str = "concise"


class AiServiceResponse(BaseModel):
    answer: str


class SummaryRequest(BaseModel):
    session_id: str = Field(alias="sessionId")
    history: list[ChatMessage] = Field(default_factory=list)
    current_summary: str | None = Field(
        alias="currentSummary",
        default=None,
    )


class SummaryResponse(BaseModel):
    summary: str
