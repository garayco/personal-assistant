from enum import Enum

from pydantic import BaseModel, Field


class MessageRole(str, Enum):
    user = "user"
    assistant = "assistant"
    system = "system"


class ChatMessage(BaseModel):
    role: MessageRole
    content: str


class ChatRequest(BaseModel):
    session_id: str = Field(alias="sessionId")
    user_message: str = Field(alias="userMessage")
    history: list[ChatMessage] = Field(default_factory=list)
    current_summary: str | None = Field(alias="currentSummary", default=None)
    task: str = "chat"
    persona: str = "personal assistant"
    tone: str = "concise"
    should_summarize: bool = Field(
        default=False,
        alias="shouldSummarize",
    )


class AiServiceResponse(BaseModel):
    answer: str
