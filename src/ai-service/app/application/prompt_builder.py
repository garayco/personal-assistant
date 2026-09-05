from app.domain.models import AiServiceRequest, SummaryRequest


def build_messages(request: AiServiceRequest) -> list[dict[str, str]]:
    messages = [
        {
            "role": "system",
            "content": (
                f"You are a {request.persona}. Respond in a {request.tone} tone. "
                "Answer in the same language as the user."
            ),
        }
    ]

    if request.current_summary:
        messages.append(
            {
                "role": "system",
                "content": f"Conversation summary:\n{request.current_summary}",
            }
        )

    messages.extend(
        {"role": message.role.value.lower(), "content": message.content}
        for message in request.history
    )
    messages.append({"role": "user", "content": request.user_message})
    return messages


def build_summary(request: SummaryRequest) -> list[dict[str, str]]:
    messages = [
        {
            "role": "system",
            "content": (
                "Summarize the conversation concisely. "
                "Preserve important facts, preferences, decisions, "
                "and pending tasks. Return only the updated summary."
            ),
        }
    ]

    if request.current_summary:
        messages.append(
            {
                "role": "system",
                "content": f"Previous summary:\n{request.current_summary}",
            }
        )

    messages.extend(
        {
            "role": message.role.value.lower(),
            "content": message.content,
        }
        for message in request.history
    )

    return messages
