namespace Api.Dto;

public record SendMessageRequest(string Content, string Collection = "default");