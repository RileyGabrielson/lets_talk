using System;

public class RedirectEvent : StoryEvent
{
    private Section sectionToRedirect;
    private Action<Section> onRedirectToSection;

    public RedirectEvent(Section sectionToRedirect, Action<Section> onRedirectToSection) {
        this.sectionToRedirect = sectionToRedirect;
        this.onRedirectToSection = onRedirectToSection;
    }

    public void Execute()
    {
        this.onRedirectToSection(sectionToRedirect);
    }
}
