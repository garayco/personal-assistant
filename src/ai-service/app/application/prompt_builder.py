from app.domain.models import ChatRequest


def build_messages(request: ChatRequest) -> list[dict[str, str]]:
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
