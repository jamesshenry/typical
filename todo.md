## Header

- margin top 2 to give some space from menubar.
- not full width
- row of several groups of buttons, example:
  - **toggles**: `[punctuation, numbers]`
  - **modes**: `[time, words, quote, zen, custom]`
  - **mode_options**: `[change]`
- **modes** group is central focal point
  - example: quote mode
    - disables **toggles** group on left
    - **mode_options**: `[all, short, medium, long, thick]` on right
- all buttons reset the typing view state to fresh with new quote
- `change` brings up an options menu

---

- add `Emit()` to IScript returning the
