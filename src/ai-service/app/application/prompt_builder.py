from app.domain.models import AiServiceRequest, SummaryRequest


def build_messages(request: AiServiceRequest) -> list[dict[str, str]]:
    system_parts = [
        f"You are an empathetic {request.persona}. Respond in a {request.tone} tone. "
        "Answer in the same language as the user. "
        "Help the user develop, track, and maintain positive habits."
    ]

    if request.active_habits:
        habits_text = "\n".join(f"- {h}" for h in request.active_habits)
        system_parts.append(f"Active user habits and streaks:\n{habits_text}")

    if request.relevant_memories:
        memories_text = "\n".join(f"- {m}" for m in request.relevant_memories)
        system_parts.append(f"Relevant long-term memories retrieved:\n{memories_text}")

    if request.current_summary:
        system_parts.append(f"Current session summary:\n{request.current_summary}")

    messages = [
        {
            "role": "system",
            "content": "\n\n".join(system_parts),
        }
    ]

    messages.extend(
        {"role": message.role.value.lower(), "content": message.content}
        for message in request.history
    )
    messages.append({"role": "user", "content": request.user_message})
    return messages


def build_summary(request: SummaryRequest) -> list[dict[str, str]]:
    system_prompt = (
        "You are an AI memory curator for a personal assistant. "
        "Your role is to analyze conversation transcripts and update memory in valid JSON."
    )

    content_parts: list[str] = []

    if request.current_summary:
        content_parts.append(f"PREVIOUS SUMMARY:\n{request.current_summary}")

    if request.existing_facts:
        facts_list = "\n".join(f"[{i + 1}] {fact}" for i, fact in enumerate(request.existing_facts))
        content_parts.append(f"ALREADY KNOWN FACTS:\n{facts_list}")
    else:
        content_parts.append("ALREADY KNOWN FACTS:\n(None)")

    transcript_lines = [
        f"{message.role.value.capitalize()}: {message.content}"
        for message in request.history
    ]
    content_parts.append("NEW CONVERSATION TRANSCRIPT:\n" + "\n".join(transcript_lines))

    instructions = (
        "TASK:\n"
        "Return a strictly valid JSON object:\n"
        "{\n"
        '  "summary": "Cohesive cumulative summary combining past summary and new developments.",\n'
        '  "new_facts": ["New facts or habits stated by the user. In 3rd person."],\n'
        '  "updated_facts": [{"id": 1, "updated": "Updated fact for this exact ID narrating past state and resolution."}]\n'
        "}\n\n"
        "RULES:\n"
        "1. 3RD PERSON ONLY: Write all facts in 3rd person (e.g. 'El usuario...'). Never use 1st person ('Me propuse').\n"
        "2. USER FACTS ONLY: Extract facts exclusively from what the User says about themselves. Ignore assistant advice.\n"
        "3. DO NOT DUPLICATE: If a fact already exists in ALREADY KNOWN FACTS and has not changed, do NOT include it in new_facts.\n"
        "4. EXACT ID MATCHING: In updated_facts, only update a fact if its specific condition changed (e.g. ankle update goes to the ankle ID). If there are no known facts to update, updated_facts MUST be []. Never return null for id.\n"
        "5. LANGUAGE: Output 100% in the language of the conversation. Do not mix English and Spanish."
    )
    content_parts.append(instructions)

    return [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": "\n\n".join(content_parts)},
    ]
